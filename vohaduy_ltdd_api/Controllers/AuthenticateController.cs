using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using vohaduy_ltdd_api.DTOs;
using vohaduy_ltdd_api.Models;
using vohaduy_ltdd_api.Services;

namespace vohaduy_ltdd_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IFaceRecognitionService _faceService;
        private readonly ILogger<AuthenticateController> _logger;

        public AuthenticateController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            IEmailService emailService,
            IFaceRecognitionService faceService,
            ILogger<AuthenticateController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
            _emailService = emailService;
            _faceService = faceService;
            _logger = logger;
        }

        /// <summary>
        /// 1. Đăng ký tài khoản qua Email kèm tạo hồ sơ nhân viên
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userByUsername = await _userManager.FindByNameAsync(model.Username);
            if (userByUsername != null)
            {
                return BadRequest(new { status = false, message = "Tên đăng nhập đã tồn tại trong hệ thống" });
            }

            var userByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userByEmail != null)
            {
                return BadRequest(new { status = false, message = "Email này đã được sử dụng" });
            }

            // Xử lý phòng ban nếu có (nếu chưa có phòng ban thì để null)
            int? departmentId = model.DepartmentId;
            if (departmentId.HasValue)
            {
                var department = await _context.Departments.FindAsync(departmentId.Value);
                if (department == null) departmentId = null;
            }

            // Tự động sinh mã nhân viên
            string empCode = $"NV{Random.Shared.Next(1000, 9999)}";

            var employee = new Employee
            {
                EmployeeCode = empCode,
                FullName = model.FullName ?? model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Position = model.Position ?? "Nhân viên",
                DepartmentId = departmentId,
                HireDate = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.FullName ?? model.Username,
                Initials = model.Initials ?? (model.Username.Length > 2 ? model.Username[..2].ToUpper() : model.Username.ToUpper()),
                EmployeeId = employee.Id,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                // rollback employee
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = false,
                    message = "Đăng ký thất bại",
                    errors = createResult.Errors.Select(e => e.Description)
                });
            }

            // Link employee with identity user
            employee.UserId = user.Id;
            employee.Role = string.IsNullOrWhiteSpace(model.Role) ? "Employee" : model.Role;
            await _context.SaveChangesAsync();

            // Gán role
            string role = employee.Role;
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            await _userManager.AddToRoleAsync(user, role);

            // Đăng ký trực tiếp thành công - KHÔNG CẦN OTP!
            return Ok(new
            {
                status = true,
                message = "Đăng ký tài khoản thành công! Bạn có thể đăng nhập ngay.",
                employeeCode = empCode,
                email = user.Email,
                role = role
            });
        }


        /// <summary>
        /// 2. Đăng nhập hệ thống bằng Tên đăng nhập/Email + Mật khẩu (Đăng nhập bình thường, không dùng OTP)
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Tìm user bằng Tên đăng nhập hoặc Email
            var user = await _userManager.FindByNameAsync(model.Username) 
                       ?? await _userManager.FindByEmailAsync(model.Username);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new
                {
                    status = false,
                    message = "Tên đăng nhập hoặc mật khẩu không chính xác"
                });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new
                {
                    status = false,
                    message = "Tài khoản của bạn đã bị khóa hoặc ngừng hoạt động"
                });
            }

            // Đăng nhập bình thường thành công -> Trả về Token JWT và thông tin người dùng ngay lập tức
            return await GenerateAuthResponse(user, "Đăng nhập thành công");
        }


        /// <summary>
        /// Đăng nhập nhanh không cần OTP
        /// </summary>
        [HttpPost("quick-login")]
        public async Task<IActionResult> QuickLogin([FromBody] LoginModel model)
        {
            return await Login(model);
        }

        /// <summary>
        /// 5.a Đăng nhập bằng Khuôn mặt (Face Recognition Login)
        /// </summary>
        [HttpPost("face-login")]
        public async Task<IActionResult> FaceLogin([FromBody] FaceLoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy tài khoản với email này" });
            }

            Employee? employee = null;
            if (user.EmployeeId.HasValue)
            {
                employee = await _context.Employees.FindAsync(user.EmployeeId.Value);
            }

            string? faceDescriptor = employee?.FaceData ?? user.FaceFeatureHash;
            if (string.IsNullOrWhiteSpace(faceDescriptor))
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Tài khoản này chưa đăng ký dữ liệu khuôn mặt. Vui lòng đăng ký khuôn mặt trước."
                });
            }

            var verifyResult = _faceService.VerifyFace(faceDescriptor, model.FaceImageBase64);
            if (!verifyResult.IsMatch)
            {
                return Unauthorized(new
                {
                    status = false,
                    confidenceScore = verifyResult.ConfidenceScore,
                    message = verifyResult.Message ?? "Khuôn mặt không trùng khớp"
                });
            }

            return await GenerateAuthResponse(user, $"Đăng nhập bằng khuôn mặt thành công (Độ tin cậy: {verifyResult.ConfidenceScore}%)");
        }


        /// <summary>
        /// 3. Phân quyền người dùng: Lấy thông tin user hiện tại + vai trò + chuyển hướng Dashboard
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound(new { status = false, message = "Không tìm thấy người dùng" });

            var roles = await _userManager.GetRolesAsync(user);
            Employee? employee = null;
            string? departmentName = null;

            if (user.EmployeeId.HasValue)
            {
                employee = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Id == user.EmployeeId.Value);

                departmentName = employee?.Department?.DepartmentName;
            }

            // 3. Phân quyền: 1 là Admin, 2 là Nhân viên
            string targetDashboard = roles.Contains("Admin") ? "AdminDashboard" : "EmployeeDashboard";

            var profile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? employee?.FullName ?? user.UserName ?? string.Empty,
                EmployeeId = user.EmployeeId,
                EmployeeCode = employee?.EmployeeCode,
                Position = employee?.Position,
                DepartmentName = departmentName,
                AvatarUrl = user.AvatarUrl ?? employee?.AvatarUrl,
                IsFaceRegistered = employee?.IsFaceRegistered ?? !string.IsNullOrEmpty(user.FaceFeatureHash),
                Roles = roles,
                TargetDashboard = targetDashboard
            };

            return Ok(new { status = true, data = profile });
        }

        private async Task<IActionResult> GenerateAuthResponse(User user, string customMessage = "Đăng nhập thành công")
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            Employee? employee = null;
            if (user.EmployeeId.HasValue)
            {
                employee = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Id == user.EmployeeId.Value);
            }

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("FullName", user.FullName ?? employee?.FullName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (user.EmployeeId.HasValue)
            {
                authClaims.Add(new Claim("EmployeeId", user.EmployeeId.Value.ToString()));
            }

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = CreateJwtToken(authClaims);

            // 3. Phân quyền: 1 là Admin, 2 là Nhân viên
            string targetDashboard = userRoles.Contains("Admin") ? "AdminDashboard" : "EmployeeDashboard";

            return Ok(new
            {
                status = true,
                message = customMessage,
                token = token,
                roles = userRoles,
                targetDashboard = targetDashboard,
                userInfo = new
                {
                    id = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    fullName = user.FullName ?? employee?.FullName,
                    employeeId = user.EmployeeId,
                    employeeCode = employee?.EmployeeCode,
                    departmentName = employee?.Department?.DepartmentName,
                    position = employee?.Position,
                    isFaceRegistered = employee?.IsFaceRegistered ?? !string.IsNullOrEmpty(user.FaceFeatureHash)
                }
            });
        }

        private string CreateJwtToken(IEnumerable<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("JWTKey");
            var secret = jwtSettings["Secret"] ?? "VoHaDuySuperSecureKeyForAttendanceAppJwtToken2026!@#";
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            double expiryHours = 24;
            if (double.TryParse(jwtSettings["TokenExpiryTimeInHour"], out var parsedExpiry))
            {
                expiryHours = parsedExpiry;
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expiryHours),
                Issuer = jwtSettings["ValidIssuer"],
                Audience = jwtSettings["ValidAudience"],
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}