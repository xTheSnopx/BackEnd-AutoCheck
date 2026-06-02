using AutoCheckAML.Api.Data;
using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Web.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutoCheckAML.Api.Business
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto> GetByIdAsync(int id);
        Task<UserDto> CreateAsync(CreateUserRequest request);
        Task<UserDto> UpdateAsync(int id, UpdateUserRequest request);
        Task<bool> DeleteAsync(int id);
        Task<bool> ChangePasswordAsync(int userId, string newPassword);
        Task<bool> AssignRoleAsync(int userId, int roleId);
        Task<bool> RevokeRoleAsync(int userId, int roleId);
    }

    public class UserService : IUserService
    {
        private readonly AutoCheckAMLContext _context;

        public UserService(AutoCheckAMLContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _context.Users
                .Include(u => u.Crew)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return users.Select(MapToDto).ToList();
        }

        public async Task<UserDto> GetByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Crew)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null)
                throw new KeyNotFoundException($"Usuario {id} no encontrado.");

            return MapToDto(user);
        }

        public async Task<UserDto> CreateAsync(CreateUserRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                throw new InvalidOperationException("El nombre de usuario ya existe.");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("El email ya está registrado.");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                CrewId = request.CrewId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Asignar roles
            if (request.RoleIds?.Any() == true)
            {
                foreach (var roleId in request.RoleIds)
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId,
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }
                await _context.SaveChangesAsync();
            }

            return await GetByIdAsync(user.Id);
        }

        public async Task<UserDto> UpdateAsync(int id, UpdateUserRequest request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null)
                throw new KeyNotFoundException($"Usuario {id} no encontrado.");

            user.Email = request.Email ?? user.Email;
            user.FullName = request.FullName ?? user.FullName;
            user.IsActive = request.IsActive;
            user.CrewId = request.CrewId;
            user.UpdatedAt = DateTime.UtcNow;

            // Actualizar roles si se especifican
            if (request.RoleIds != null)
            {
                // Desactivar roles actuales
                foreach (var ur in user.UserRoles)
                    ur.IsActive = false;

                // Asignar nuevos roles
                foreach (var roleId in request.RoleIds)
                {
                    var existing = user.UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
                    if (existing != null)
                        existing.IsActive = true;
                    else
                        _context.UserRoles.Add(new UserRole
                        {
                            UserId = id,
                            RoleId = roleId,
                            AssignedAt = DateTime.UtcNow,
                            IsActive = true
                        });
                }
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) throw new KeyNotFoundException($"Usuario {id} no encontrado.");

            // Soft delete
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new KeyNotFoundException("Usuario no encontrado.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignRoleAsync(int userId, int roleId)
        {
            var existing = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (existing != null)
            {
                existing.IsActive = true;
            }
            else
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevokeRoleAsync(int userId, int roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole == null) return false;

            userRole.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        private static UserDto MapToDto(User u) => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            FullName = u.FullName,
            IsActive = u.IsActive,
            LastLogin = u.LastLogin,
            CrewId = u.CrewId,
            CrewName = u.Crew?.Name,
            Roles = u.UserRoles?.Where(ur => ur.IsActive).Select(ur => ur.Role?.Name).Where(n => n != null).ToList() ?? new List<string>(),
            CreatedAt = u.CreatedAt
        };
    }
}
