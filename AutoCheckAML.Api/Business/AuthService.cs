using AutoCheckAML.Api.Data;
using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Web.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AutoCheckAML.Api.Business
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(int userId, string token);
    }

    public class AuthService : IAuthService
    {
        private readonly AutoCheckAMLContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(AutoCheckAMLContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Usuario y contraseña son requeridos.");

            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Username == request.Username && !u.IsDeleted);

            if (user == null)
                throw new UnauthorizedAccessException("Usuario o contraseña inválidos.");

            // Verify password - support both BCrypt hash and plain text for database testing
            bool isPasswordValid = false;
            try
            {
                // Try BCrypt verification first
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            }
            catch (Exception)
            {
                // If BCrypt fails, check if it's plain text (for database testing/development)
                // BCrypt hashes are always 60 characters, so if it's shorter, it might be plain text
                if (user.PasswordHash.Length < 60)
                {
                    isPasswordValid = user.PasswordHash == request.Password;

                    // If plain text matches, auto-upgrade to BCrypt hash
                    if (isPasswordValid)
                    {
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            if (!isPasswordValid)
                throw new UnauthorizedAccessException("Usuario o contraseña inválidos.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Usuario inactivo. Contacte al administrador.");

            // Obtener roles y permisos activos
            var roles = user.UserRoles
                .Where(ur => ur.IsActive)
                .Select(ur => ur.Role.Name)
                .Distinct().ToList();

            var permissions = user.UserRoles
                .Where(ur => ur.IsActive)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Where(rp => rp.IsActive)
                .Select(rp => rp.Permission.Name)
                .Distinct().ToList();

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var accessToken = GenerateAccessToken(user, roles, permissions);
            var refreshToken = GenerateRefreshToken();

            // Guardar refresh token en BD
            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IssuedFromIp = GetClientIpAddress()
            });
            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Token = accessToken,
                RefreshToken = refreshToken,
                Roles = roles,
                Permissions = permissions
            };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            if (_context.Users.Any(u => u.Username == request.Username))
                throw new InvalidOperationException("El usuario ya existe.");

            if (_context.Users.Any(u => u.Email == request.Email))
                throw new InvalidOperationException("El email ya está registrado.");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Asignar rol CUADRILLA por defecto (HierarchyLevel=3)
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "CUADRILLA");
            if (defaultRole != null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = defaultRole.Id,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                });
                await _context.SaveChangesAsync();
            }

            return new RegisterResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Message = "Usuario registrado exitosamente."
            };
        }

        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                            .ThenInclude(r => r.RolePermissions)
                                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.IsExpired)
                throw new UnauthorizedAccessException("Refresh token inválido o expirado.");

            // Revocar token anterior
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            var user = storedToken.User;
            var roles = user.UserRoles.Where(ur => ur.IsActive).Select(ur => ur.Role.Name).Distinct().ToList();
            var permissions = user.UserRoles.Where(ur => ur.IsActive)
                .SelectMany(ur => ur.Role.RolePermissions).Where(rp => rp.IsActive)
                .Select(rp => rp.Permission.Name).Distinct().ToList();

            var newAccessToken = GenerateAccessToken(user, roles, permissions);
            var newRefreshToken = GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Roles = roles,
                Permissions = permissions
            };
        }

        public async Task<bool> RevokeTokenAsync(int userId, string token)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.UserId == userId);

            if (storedToken == null) return false;

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateAccessToken(User user, List<string> roles, List<string> permissions)
        {
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? _configuration["Jwt:Secret"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            var key = Encoding.ASCII.GetBytes(jwtSecret);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            foreach (var perm in permissions)
                claims.Add(new Claim("permission", perm));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private string GetClientIpAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return "Unknown";

                // Check for forwarded IP (when behind proxy/load balancer)
                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    var ips = forwardedFor.Split(',');
                    if (ips.Length > 0)
                        return ips[0].Trim();
                }

                // Get direct connection IP
                return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
