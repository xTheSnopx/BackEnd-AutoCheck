using AutoCheckAML.Api.Data;
using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Web.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutoCheckAML.Api.Business
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetAllAsync();
        Task<RoleDto> GetByIdAsync(int id);
        Task<bool> AssignPermissionAsync(int roleId, int permissionId);
        Task<bool> RevokePermissionAsync(int roleId, int permissionId);
        Task<List<PermissionDto>> GetAllPermissionsAsync();
    }

    public class RoleService : IRoleService
    {
        private readonly AutoCheckAMLContext _context;

        public RoleService(AutoCheckAMLContext context)
        {
            _context = context;
        }

        public async Task<List<RoleDto>> GetAllAsync()
        {
            var roles = await _context.Roles
                .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .Where(r => r.IsActive)
                .OrderBy(r => r.HierarchyLevel)
                .ToListAsync();

            return roles.Select(MapToDto).ToList();
        }

        public async Task<RoleDto> GetByIdAsync(int id)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

            if (role == null)
                throw new KeyNotFoundException($"Rol {id} no encontrado.");

            return MapToDto(role);
        }

        public async Task<bool> AssignPermissionAsync(int roleId, int permissionId)
        {
            var existing = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (existing != null)
            {
                existing.IsActive = true;
            }
            else
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    GrantedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevokePermissionAsync(int roleId, int permissionId)
        {
            var rp = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rp == null) return false;

            rp.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category).ThenBy(p => p.Name)
                .ToListAsync();

            return permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category,
                IsCritical = p.IsCritical,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }).ToList();
        }

        private static RoleDto MapToDto(Role r) => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            HierarchyLevel = r.HierarchyLevel,
            IsSystemRole = r.IsSystemRole,
            IsActive = r.IsActive,
            Permissions = r.RolePermissions?
                .Where(rp => rp.IsActive && rp.Permission != null)
                .Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    Description = rp.Permission.Description,
                    Category = rp.Permission.Category,
                    IsCritical = rp.Permission.IsCritical,
                    IsActive = rp.Permission.IsActive,
                    CreatedAt = rp.Permission.CreatedAt
                })
                .ToList() ?? new List<PermissionDto>(),
            CreatedAt = r.CreatedAt
        };
    }
}
