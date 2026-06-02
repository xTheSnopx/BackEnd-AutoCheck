using AutoCheckAML.Api.Data;
using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Web.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoCheckAML.Api.Business
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly AutoCheckAMLContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AutoCheckAMLContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    throw new ArgumentException("Usuario y contraseña son requeridos");
                }

                var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Usuario o contraseña inválidos");
                }

                if (!user.IsActive)
                {
                    throw new UnauthorizedAccessException("Usuario inactivo");
                }

                user.LastLogin = DateTime.Now;
                _context.SaveChanges();

                var token = GenerateJwtToken(user);

                return new LoginResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Token = token
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en login: {ex.Message}");
            }
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(request.Username) || 
                    string.IsNullOrWhiteSpace(request.Email) || 
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.FullName))
                {
                    throw new ArgumentException("Todos los campos son requeridos");
                }

                // Verificar si usuario existe
                if (_context.Users.Any(u => u.Username == request.Username))
                {
                    throw new InvalidOperationException("El usuario ya existe");
                }

                if (_context.Users.Any(u => u.Email == request.Email))
                {
                    throw new InvalidOperationException("El email ya está registrado");
                }

                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FullName = request.FullName,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    LastLogin = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return new RegisterResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Message = "Usuario registrado exitosamente"
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en registro: {ex.Message}");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSecret = _configuration["Jwt:Secret"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                throw new InvalidOperationException("JWT configuration is missing");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
