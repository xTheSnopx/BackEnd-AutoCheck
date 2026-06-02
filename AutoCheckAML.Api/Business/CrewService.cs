using AutoCheckAML.Api.Data;
using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Web.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutoCheckAML.Api.Business
{
    public interface ICrewService
    {
        Task<List<CrewDto>> GetAllAsync();
        Task<CrewDto> GetByIdAsync(int id);
        Task<CrewDto> CreateAsync(int userId, CreateCrewRequest request);
        Task<CrewDto> UpdateAsync(int id, UpdateCrewRequest request);
        Task<bool> DeleteAsync(int id);
        Task<bool> AddMemberAsync(int crewId, int userId);
        Task<bool> RemoveMemberAsync(int crewId, int userId);
        Task<List<UserDto>> GetMembersAsync(int crewId);
    }

    public class CrewService : ICrewService
    {
        private readonly AutoCheckAMLContext _context;

        public CrewService(AutoCheckAMLContext context)
        {
            _context = context;
        }

        public async Task<List<CrewDto>> GetAllAsync()
        {
            var crews = await _context.Crews
                .Include(c => c.ManagedByUser)
                .Include(c => c.Members)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return crews.Select(MapToDto).ToList();
        }

        public async Task<CrewDto> GetByIdAsync(int id)
        {
            var crew = await _context.Crews
                .Include(c => c.ManagedByUser)
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (crew == null)
                throw new KeyNotFoundException($"Cuadrilla {id} no encontrada.");

            return MapToDto(crew);
        }

        public async Task<CrewDto> CreateAsync(int userId, CreateCrewRequest request)
        {
            if (await _context.Crews.AnyAsync(c => c.Name == request.Name && c.IsActive))
                throw new InvalidOperationException($"Ya existe una cuadrilla con el nombre '{request.Name}'.");

            var crew = new Crew
            {
                Name = request.Name,
                Description = request.Description,
                ManagedByUserId = request.ManagedByUserId,
                Department = request.Department,
                Location = request.Location,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                MemberCount = 0
            };

            _context.Crews.Add(crew);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(crew.Id);
        }

        public async Task<CrewDto> UpdateAsync(int id, UpdateCrewRequest request)
        {
            var crew = await _context.Crews.FindAsync(id);
            if (crew == null || !crew.IsActive)
                throw new KeyNotFoundException($"Cuadrilla {id} no encontrada.");

            crew.Name = request.Name ?? crew.Name;
            crew.Description = request.Description ?? crew.Description;
            crew.ManagedByUserId = request.ManagedByUserId;
            crew.Department = request.Department ?? crew.Department;
            crew.Location = request.Location ?? crew.Location;
            crew.IsActive = request.IsActive;
            crew.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var crew = await _context.Crews.FindAsync(id);
            if (crew == null) throw new KeyNotFoundException($"Cuadrilla {id} no encontrada.");

            crew.IsActive = false;
            crew.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddMemberAsync(int crewId, int userId)
        {
            var crew = await _context.Crews.FindAsync(crewId);
            if (crew == null || !crew.IsActive)
                throw new KeyNotFoundException($"Cuadrilla {crewId} no encontrada.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
                throw new KeyNotFoundException($"Usuario {userId} no encontrado.");

            user.CrewId = crewId;
            crew.MemberCount = await _context.Users.CountAsync(u => u.CrewId == crewId && u.IsActive);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveMemberAsync(int crewId, int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.CrewId == crewId);
            if (user == null) return false;

            user.CrewId = null;

            var crew = await _context.Crews.FindAsync(crewId);
            if (crew != null)
                crew.MemberCount = await _context.Users.CountAsync(u => u.CrewId == crewId && u.IsActive && u.Id != userId);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UserDto>> GetMembersAsync(int crewId)
        {
            var members = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Crew)
                .Where(u => u.CrewId == crewId && u.IsActive && !u.IsDeleted)
                .ToListAsync();

            return members.Select(u => new UserDto
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
            }).ToList();
        }

        private static CrewDto MapToDto(Crew c) => new CrewDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            ManagedByUserId = c.ManagedByUserId,
            ManagedByUserName = c.ManagedByUser?.FullName,
            Department = c.Department,
            Location = c.Location,
            MemberCount = c.MemberCount,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        };
    }
}
