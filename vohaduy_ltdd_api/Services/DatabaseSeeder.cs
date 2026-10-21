using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using vohaduy_ltdd_api.Models;

namespace vohaduy_ltdd_api.Services
{
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Tự động tạo schema database nếu chưa tồn tại
                await _context.Database.EnsureCreatedAsync();

                // Chỉ đảm bảo 2 Role hệ thống (Admin, Employee) tồn tại để người dùng tự đăng ký thật
                string[] roles = { "Admin", "Employee" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                _logger.LogInformation("Database initialized cleanly. Không tạo bất kỳ dữ liệu ảo nào.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình khởi tạo cấu trúc dữ liệu");
            }
        }
    }
}
