using AutoCheckAML.Api.Data;
using AutoCheckAML.Api.Entity;
using Microsoft.EntityFrameworkCore;

namespace AutoCheckAML.Api.Business
{
    public interface IAuditService
    {
        Task LogAsync(int userId, string entityName, int entityId, string action, string description,
            string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null);
        Task LogLoginAsync(int userId, string ipAddress, string userAgent, bool success);
        Task LogLogoutAsync(int userId, string ipAddress, string userAgent);
        Task<List<AuditLog>> GetByUserAsync(int userId, int limit = 50);
        Task<List<AuditLog>> GetRecentAsync(int limit = 100);
    }

    public class AuditService : IAuditService
    {
        private readonly AutoCheckAMLContext _context;

        public AuditService(AutoCheckAMLContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int userId, string entityName, int entityId, string action, string description,
            string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null)
        {
            var log = new AuditLog
            {
                UserId = userId,
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                Description = description,
                OldValues = oldValues ?? string.Empty,
                NewValues = newValues ?? string.Empty,
                IpAddress = ipAddress ?? "Unknown",
                UserAgent = userAgent ?? "Unknown",
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task LogLoginAsync(int userId, string ipAddress, string userAgent, bool success)
        {
            var action = success ? "Login" : "LoginFailed";
            var description = success
                ? "Inicio de sesión exitoso"
                : "Intento de inicio de sesión fallido";

            await LogAsync(userId, "Session", userId, action, description,
                ipAddress: ipAddress, userAgent: userAgent);
        }

        public async Task LogLogoutAsync(int userId, string ipAddress, string userAgent)
        {
            await LogAsync(userId, "Session", userId, "Logout", "Cierre de sesión",
                ipAddress: ipAddress, userAgent: userAgent);
        }

        public async Task<List<AuditLog>> GetByUserAsync(int userId, int limit = 50)
        {
            return await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetRecentAsync(int limit = 100)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}
