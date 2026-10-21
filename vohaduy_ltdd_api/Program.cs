using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using vohaduy_ltdd_api.Models;
using vohaduy_ltdd_api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình CSDL SQL Server với tên DB cá nhân: vohaduy_ltdd_api
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình Identity cho User
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Đăng ký Dependency Injection các Dịch vụ Nghiệp vụ Chấm Công
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IFaceRecognitionService, FaceRecognitionService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();

// Đăng ký SignalR phục vụ Gọi video nhóm và Chat thời gian thực (WebRTC Signaling)
builder.Services.AddSignalR();

// 4. Controllers & Cấu hình JSON Serialization
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// 5. Cấu hình Swagger/OpenAPI kèm xác thực JWT Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "vohaduy_ltdd_api - Hệ Thống API Chấm Công Nhân Viên Di Động",
        Version = "v1",
        Description = "Backend Web API phục vụ Ứng dụng Di Động Chấm Công Nhân Viên\n" +
                      "- Tác giả: Võ Hà Duy\n" +
                      "- Chức năng hệ thống: Đăng ký tài khoản, Đăng nhập bình thường (Username & Mật khẩu), Phân quyền Role (Admin/Employee), CRUD Nhân viên/Phòng ban/Ca làm/Đơn từ.\n" +
                      "- 3 Chức năng thuyết trình lựa chọn (3.5đ):\n" +
                      "  5.a: Đăng nhập bằng khuôn mặt\n" +
                      "  5.b: Chia sẻ vị trí (GPS Geofencing & Live Location)\n" +
                      "  5.c: Gọi video nhóm (theo mã phòng WebRTC)"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập mã JWT Token vào ô dưới đây theo cú pháp: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 6. Cấu hình CORS cho Mobile App và Web Client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 7. Cấu hình JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JWTKey");
var secretKey = jwtSettings["Secret"] ?? "VoHaDuySuperSecureKeyForAttendanceAppJwtToken2026!@#";
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// 8. Tự động khởi tạo Database và Dữ liệu mẫu (Database Seeding)
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    await seeder.SeedAsync();
}

// 9. Cấu hình Middleware Pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "vohaduy_ltdd_api v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<vohaduy_ltdd_api.Hubs.ChatAndVideoHub>("/hubs/chat");

// Chuyển hướng trang chủ về /swagger để mở ngay giao diện API
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();