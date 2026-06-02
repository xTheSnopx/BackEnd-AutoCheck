using AutoCheckAML.Api.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AutoCheckAML.Api.Data
{
    /// <summary>
    /// DbContext principal de AutoCheckAML.
    /// Incluye todas las entidades del sistema: usuarios, roles, formularios, auditoría, etc.
    /// </summary>
    public class AutoCheckAMLContext : DbContext
    {
        public AutoCheckAMLContext(DbContextOptions<AutoCheckAMLContext> options) : base(options)
        {
        }

        // ========== ENTIDADES DE SEGURIDAD ==========
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // ========== ENTIDADES DE ORGANIZACIÓN ==========
        public DbSet<Crew> Crews { get; set; }

        // ========== ENTIDADES DE FORMULARIOS ==========
        public DbSet<FormTemplate> FormTemplates { get; set; }
        public DbSet<FormField> FormFields { get; set; }
        public DbSet<FormSubmission> FormSubmissions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        // ========== ENTIDADES DE AUDITORÍA Y EXPORTACIÓN ==========
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ExportHistory> ExportHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== CONFIGURACIÓN DE ENTIDADES DE SEGURIDAD ==========

            // UserRole: Relación M2M entre User y Role
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // RolePermission: Relación M2M entre Role y Permission
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // RefreshToken: Relación entre User y RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========== CONFIGURACIÓN DE ENTIDADES DE ORGANIZACIÓN ==========

            // Crew: Relación con User (ManagedByUser)
            modelBuilder.Entity<Crew>()
                .HasOne(c => c.ManagedByUser)
                .WithMany()
                .HasForeignKey(c => c.ManagedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Crew (Members)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Crew)
                .WithMany(c => c.Members)
                .HasForeignKey(u => u.CrewId)
                .OnDelete(DeleteBehavior.SetNull);

            // ========== CONFIGURACIÓN DE ENTIDADES DE FORMULARIOS ==========

            // FormTemplate -> User (CreatedByUser)
            modelBuilder.Entity<FormTemplate>()
                .HasOne(ft => ft.CreatedByUser)
                .WithMany()
                .HasForeignKey(ft => ft.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormField -> FormTemplate
            modelBuilder.Entity<FormField>()
                .HasOne(ff => ff.FormTemplate)
                .WithMany(ft => ft.FormFields)
                .HasForeignKey(ff => ff.FormTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // FormSubmission -> FormTemplate
            modelBuilder.Entity<FormSubmission>()
                .HasOne(fs => fs.FormTemplate)
                .WithMany(ft => ft.FormSubmissions)
                .HasForeignKey(fs => fs.FormTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormSubmission -> User (SubmittedByUser)
            modelBuilder.Entity<FormSubmission>()
                .HasOne(fs => fs.SubmittedByUser)
                .WithMany(u => u.FormSubmissions)
                .HasForeignKey(fs => fs.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormSubmission -> User (VerifiedByUser)
            modelBuilder.Entity<FormSubmission>()
                .HasOne(fs => fs.VerifiedByUser)
                .WithMany()
                .HasForeignKey(fs => fs.VerifiedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // FormSubmission -> User (ClosedByUser)
            modelBuilder.Entity<FormSubmission>()
                .HasOne(fs => fs.ClosedByUser)
                .WithMany()
                .HasForeignKey(fs => fs.ClosedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // FormSubmission -> Crew
            modelBuilder.Entity<FormSubmission>()
                .HasOne(fs => fs.AssignedToCrew)
                .WithMany(c => c.FormSubmissions)
                .HasForeignKey(fs => fs.AssignedToCrewId)
                .OnDelete(DeleteBehavior.SetNull);

            // Answer -> FormSubmission
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.FormSubmission)
                .WithMany(fs => fs.Answers)
                .HasForeignKey(a => a.FormSubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Answer -> FormField
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.FormField)
                .WithMany(ff => ff.Answers)
                .HasForeignKey(a => a.FormFieldId)
                .OnDelete(DeleteBehavior.Restrict);

            // Attachment -> FormSubmission
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.FormSubmission)
                .WithMany(fs => fs.Attachments)
                .HasForeignKey(a => a.FormSubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Attachment -> FormField (opcional)
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.FormField)
                .WithMany()
                .HasForeignKey(a => a.FormFieldId)
                .OnDelete(DeleteBehavior.SetNull);

            // Attachment -> User (UploadedByUser)
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.UploadedByUser)
                .WithMany()
                .HasForeignKey(a => a.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== CONFIGURACIÓN DE ENTIDADES DE AUDITORÍA ==========

            // AuditLog -> User
            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExportHistory -> User
            modelBuilder.Entity<ExportHistory>()
                .HasOne(eh => eh.ExportedByUser)
                .WithMany()
                .HasForeignKey(eh => eh.ExportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== ÍNDICES ==========

            // User índices
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.CrewId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.IsActive);

            // Role índices
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.HierarchyLevel);

            // Permission índices
            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.Category);

            // Crew índices
            modelBuilder.Entity<Crew>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Crew>()
                .HasIndex(c => c.Department);

            // FormTemplate índices
            modelBuilder.Entity<FormTemplate>()
                .HasIndex(ft => ft.Name);

            modelBuilder.Entity<FormTemplate>()
                .HasIndex(ft => ft.FormType);

            // FormSubmission índices
            modelBuilder.Entity<FormSubmission>()
                .HasIndex(fs => fs.FormTemplateId);

            modelBuilder.Entity<FormSubmission>()
                .HasIndex(fs => fs.SubmittedByUserId);

            modelBuilder.Entity<FormSubmission>()
                .HasIndex(fs => fs.Status);

            modelBuilder.Entity<FormSubmission>()
                .HasIndex(fs => fs.CreatedAt);

            modelBuilder.Entity<FormSubmission>()
                .HasIndex(fs => new { fs.SubmittedByUserId, fs.CreatedAt });

            // RefreshToken índices
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.ExpiresAt);

            // AuditLog índices
            modelBuilder.Entity<AuditLog>()
                .HasIndex(al => al.UserId);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(al => al.EntityName);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(al => al.Action);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(al => al.CreatedAt);

            // ExportHistory índices
            modelBuilder.Entity<ExportHistory>()
                .HasIndex(eh => eh.ExportedByUserId);

            modelBuilder.Entity<ExportHistory>()
                .HasIndex(eh => eh.CreatedAt);

            // ========== SEED DATA ==========
            SeedRoles(modelBuilder);
            SeedPermissions(modelBuilder);
            SeedRolePermissions(modelBuilder);
            SeedAdminUser(modelBuilder);
        }

        /// <summary>
        /// Siembra datos iniciales de roles.
        /// </summary>
        private void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "DEV", Description = "Desarrollador - Control total del sistema", HierarchyLevel = 0, IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Role { Id = 2, Name = "SOFTWARE", Description = "Administrador de Software - Gestión de usuarios y configuración", HierarchyLevel = 1, IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Role { Id = 3, Name = "INGENIERO_MECANICO", Description = "Ingeniero Mecánico - Responde formularios mecánicos", HierarchyLevel = 2, IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Role { Id = 4, Name = "INGENIERO_HSQ", Description = "Ingeniero HSQ - Responde formularios de HSQ", HierarchyLevel = 2, IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Role { Id = 5, Name = "CUADRILLA", Description = "Cuadrilla - Verifica y rectifica información", HierarchyLevel = 3, IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow }
            );
        }

        /// <summary>
        /// Siembra datos iniciales de permisos.
        /// </summary>
        private void SeedPermissions(ModelBuilder modelBuilder)
        {
            var permissions = new Permission[]
            {
                // User Management
                new Permission { Id = 1, Name = "CREATE_USER", Description = "Crear nuevo usuario", Category = "User Management", IsCritical = true, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 2, Name = "EDIT_USER", Description = "Editar usuario existente", Category = "User Management", IsCritical = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 3, Name = "DELETE_USER", Description = "Eliminar usuario", Category = "User Management", IsCritical = true, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 4, Name = "VIEW_USER", Description = "Ver detalles del usuario", Category = "User Management", IsCritical = false, IsActive = true, CreatedAt = DateTime.UtcNow },

                // Crew Management
                new Permission { Id = 5, Name = "CREATE_CREW", Description = "Crear nueva cuadrilla", Category = "Crew Management", IsCritical = true, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 6, Name = "EDIT_CREW", Description = "Editar cuadrilla", Category = "Crew Management", IsCritical = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 7, Name = "DELETE_CREW", Description = "Eliminar cuadrilla", Category = "Crew Management", IsCritical = true, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 8, Name = "VIEW_CREW", Description = "Ver cuadrilla", Category = "Crew Management", IsCritical = false, IsActive = true, CreatedAt = DateTime.UtcNow },

                // Form Management
                new Permission { Id = 9, Name = "CREATE_FORM", Description = "Crear nuevo formulario", Category = "Form Management", IsCritical = true, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 10, Name = "EDIT_FORM", Description = "Editar formulario", Category = "Form Management", IsCritical = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 11, Name = "SUBMIT_FORM", Description = "Responder/diligenciar formulario", Category = "Form Management", IsCritical = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 12, Name = "VERIFY_FORM", Description = "Verificar y rectificar formulario", Category = "Form Management", IsCritical = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 13, Name = "VIEW_FORM", Description = "Ver formulario", Category = "Form Management", IsCritical = false, IsActive = true, CreatedAt = DateTime.UtcNow },

                // Reports & Export
                new Permission { Id = 14, Name = "EXPORT_EXCEL", Description = "Exportar a Excel", Category = "Reports & Export", IsCritical = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 15, Name = "EXPORT_PDF", Description = "Exportar a PDF", Category = "Reports & Export", IsCritical = false, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 16, Name = "VIEW_REPORTS", Description = "Ver reportes", Category = "Reports & Export", IsCritical = false, IsActive = true, CreatedAt = DateTime.UtcNow },

                // Audit & Security
                new Permission { Id = 17, Name = "VIEW_AUDIT_LOG", Description = "Ver registro de auditoría", Category = "Audit & Security", IsCritical = true, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 18, Name = "MANAGE_ROLES", Description = "Gestionar roles y permisos", Category = "Audit & Security", IsCritical = true, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Permission { Id = 19, Name = "VIEW_EXPORT_HISTORY", Description = "Ver historial de exportaciones", Category = "Audit & Security", IsCritical = false, IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            modelBuilder.Entity<Permission>().HasData(permissions);
        }

        /// <summary>
        /// Siembra datos iniciales de asignación de permisos a roles.
        /// </summary>
        private void SeedRolePermissions(ModelBuilder modelBuilder)
        {
            // DEV (Rol 1) tiene todos los permisos
            var devPermissions = Enumerable.Range(1, 19).Select(id => new RolePermission { RoleId = 1, PermissionId = id, GrantedAt = DateTime.UtcNow, IsActive = true }).ToList();

            // SOFTWARE (Rol 2) - Admin limitado
            var softwarePermissions = new[]
            {
                new RolePermission { RoleId = 2, PermissionId = 1, GrantedAt = DateTime.UtcNow, IsActive = true },  // CREATE_USER
                new RolePermission { RoleId = 2, PermissionId = 2, GrantedAt = DateTime.UtcNow, IsActive = true },  // EDIT_USER
                new RolePermission { RoleId = 2, PermissionId = 3, GrantedAt = DateTime.UtcNow, IsActive = true },  // DELETE_USER
                new RolePermission { RoleId = 2, PermissionId = 4, GrantedAt = DateTime.UtcNow, IsActive = true },  // VIEW_USER
                new RolePermission { RoleId = 2, PermissionId = 5, GrantedAt = DateTime.UtcNow, IsActive = true },  // CREATE_CREW
                new RolePermission { RoleId = 2, PermissionId = 6, GrantedAt = DateTime.UtcNow, IsActive = true },  // EDIT_CREW
                new RolePermission { RoleId = 2, PermissionId = 7, GrantedAt = DateTime.UtcNow, IsActive = true },  // DELETE_CREW
                new RolePermission { RoleId = 2, PermissionId = 8, GrantedAt = DateTime.UtcNow, IsActive = true },  // VIEW_CREW
                new RolePermission { RoleId = 2, PermissionId = 9, GrantedAt = DateTime.UtcNow, IsActive = true },  // CREATE_FORM
                new RolePermission { RoleId = 2, PermissionId = 10, GrantedAt = DateTime.UtcNow, IsActive = true }, // EDIT_FORM
                new RolePermission { RoleId = 2, PermissionId = 13, GrantedAt = DateTime.UtcNow, IsActive = true }, // VIEW_FORM
                new RolePermission { RoleId = 2, PermissionId = 14, GrantedAt = DateTime.UtcNow, IsActive = true }, // EXPORT_EXCEL
                new RolePermission { RoleId = 2, PermissionId = 15, GrantedAt = DateTime.UtcNow, IsActive = true }, // EXPORT_PDF
                new RolePermission { RoleId = 2, PermissionId = 16, GrantedAt = DateTime.UtcNow, IsActive = true }, // VIEW_REPORTS
                new RolePermission { RoleId = 2, PermissionId = 17, GrantedAt = DateTime.UtcNow, IsActive = true }, // VIEW_AUDIT_LOG
                new RolePermission { RoleId = 2, PermissionId = 18, GrantedAt = DateTime.UtcNow, IsActive = true }, // MANAGE_ROLES
                new RolePermission { RoleId = 2, PermissionId = 19, GrantedAt = DateTime.UtcNow, IsActive = true }  // VIEW_EXPORT_HISTORY
            };

            // INGENIERO (Rol 3 y 4) - Limitado a formularios
            var ingenieroPermissions = new[]
            {
                new RolePermission { RoleId = 3, PermissionId = 11, GrantedAt = DateTime.UtcNow, IsActive = true }, // SUBMIT_FORM
                new RolePermission { RoleId = 3, PermissionId = 13, GrantedAt = DateTime.UtcNow, IsActive = true }, // VIEW_FORM
                new RolePermission { RoleId = 3, PermissionId = 14, GrantedAt = DateTime.UtcNow, IsActive = true }, // EXPORT_EXCEL
                new RolePermission { RoleId = 3, PermissionId = 15, GrantedAt = DateTime.UtcNow, IsActive = true }, // EXPORT_PDF
                new RolePermission { RoleId = 4, PermissionId = 11, GrantedAt = DateTime.UtcNow, IsActive = true }, // SUBMIT_FORM
                new RolePermission { RoleId = 4, PermissionId = 13, GrantedAt = DateTime.UtcNow, IsActive = true }, // VIEW_FORM
                new RolePermission { RoleId = 4, PermissionId = 14, GrantedAt = DateTime.UtcNow, IsActive = true }, // EXPORT_EXCEL
                new RolePermission { RoleId = 4, PermissionId = 15, GrantedAt = DateTime.UtcNow, IsActive = true }  // EXPORT_PDF
            };

            // CUADRILLA (Rol 5) - Solo verificación
            var cuadrillaPermissions = new[]
            {
                new RolePermission { RoleId = 5, PermissionId = 12, GrantedAt = DateTime.UtcNow, IsActive = true }, // VERIFY_FORM
                new RolePermission { RoleId = 5, PermissionId = 13, GrantedAt = DateTime.UtcNow, IsActive = true }  // VIEW_FORM
            };

            var allRolePermissions = devPermissions
                .Concat(softwarePermissions)
                .Concat(ingenieroPermissions)
                .Concat(cuadrillaPermissions)
                .ToList();

            modelBuilder.Entity<RolePermission>().HasData(allRolePermissions);
        }

        /// <summary>
        /// Siembra datos iniciales del usuario administrador.
        /// </summary>
        private void SeedAdminUser(ModelBuilder modelBuilder)
        {
            var now = DateTime.UtcNow;
            
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@autocheck.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FullName = "Administrador del Sistema",
                    IsActive = true,
                    CrewId = null,
                    CreatedAt = now,
                    UpdatedAt = null,
                    DeletedAt = null,
                    IsDeleted = false,
                    LastLogin = null,
                    LastModifiedBy = null,
                    DeletedBy = null
                }
            );

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = 1, RoleId = 2, AssignedAt = now, ExpiresAt = null, IsActive = true } // Usuario admin con rol SOFTWARE
            );
        }
    }
}
