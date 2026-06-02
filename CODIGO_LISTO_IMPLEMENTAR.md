# 💻 CÓDIGO LISTO PARA IMPLEMENTAR
## AutoCheckAML - 14 Nuevas Entidades v2.0

> Este documento contiene código C# listo para copiar/pegar en tu proyecto.
> Todos los archivos están listos para crear en `Entity/`

---

## 1️⃣ BASE CLASSES (Crear primero)

### BaseEntity.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Clase base para todas las entidades
    /// Propiedades: Id, CreatedAt
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Identificador único
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fecha de creación (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

### AuditableEntity.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Clase base auditable con soft delete
    /// Propiedades: UpdatedAt, UpdatedBy, DeletedAt, DeletedBy, IsDeleted
    /// </summary>
    public abstract class AuditableEntity : BaseEntity
    {
        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// ID del usuario que hizo la última actualización
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// Fecha de eliminación (soft delete)
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// ID del usuario que eliminó
        /// </summary>
        public int? DeletedBy { get; set; }

        /// <summary>
        /// Flag de soft delete (0 = activo, 1 = eliminado)
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}
```

---

## 2️⃣ ENTIDADES DE SEGURIDAD

### Role.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Rol del sistema (Admin, Manager, User, etc.)
    /// Auditable: Sí | SoftDelete: Sí
    /// </summary>
    public class Role : AuditableEntity
    {
        /// <summary>
        /// Nombre único del rol (ej: "Admin", "Manager", "User")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del rol y sus responsabilidades
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indica si el rol está activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        /// <summary>
        /// Usuarios asignados a este rol
        /// </summary>
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>
        /// Permisos asignados a este rol
        /// </summary>
        public ICollection<RolePermissionMapping> RolePermissions { get; set; } = 
            new List<RolePermissionMapping>();

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("El nombre del rol es requerido");
            if (Name.Length > 50)
                throw new ArgumentException("El nombre no puede exceder 50 caracteres");
        }
    }
}
```

### Permission.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Permiso del sistema (CREATE, READ, UPDATE, DELETE, EXPORT, etc.)
    /// Auditable: No | SoftDelete: No
    /// </summary>
    public class Permission : BaseEntity
    {
        /// <summary>
        /// Código único del permiso (ej: "FORM_CREATE", "USER_DELETE", "EXPORT")
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Nombre amigable del permiso
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción detallada del permiso
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Recurso al que aplica (ej: "Form", "User", "Role", "Report")
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Acción permitida (ej: "CREATE", "READ", "UPDATE", "DELETE", "EXPORT")
        /// </summary>
        public string Action { get; set; }

        // Navigation properties
        /// <summary>
        /// Roles que tienen este permiso
        /// </summary>
        public ICollection<RolePermissionMapping> RolePermissions { get; set; } = 
            new List<RolePermissionMapping>();

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Code))
                throw new ArgumentException("El código de permiso es requerido");
            if (string.IsNullOrWhiteSpace(Resource))
                throw new ArgumentException("El recurso es requerido");
            if (string.IsNullOrWhiteSpace(Action))
                throw new ArgumentException("La acción es requerida");
        }
    }
}
```

### UserRole.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Asignación de roles a usuarios (relación N:N)
    /// Auditable: No | SoftDelete: No
    /// </summary>
    public class UserRole : BaseEntity
    {
        /// <summary>
        /// ID del usuario
        /// Foreign Key a User
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// ID del rol
        /// Foreign Key a Role
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Fecha de asignación del rol
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID del usuario que hizo la asignación
        /// </summary>
        public int AssignedBy { get; set; }

        /// <summary>
        /// Fecha de revocación del rol (si aplica)
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// ID del usuario que revocó el rol
        /// </summary>
        public int? RevokedBy { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Role Role { get; set; }

        public void Validate()
        {
            if (UserId <= 0)
                throw new ArgumentException("UserId debe ser válido");
            if (RoleId <= 0)
                throw new ArgumentException("RoleId debe ser válido");
        }
    }
}
```

### RolePermissionMapping.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Asignación de permisos a roles (relación N:N)
    /// Auditable: No | SoftDelete: No
    /// </summary>
    public class RolePermissionMapping : BaseEntity
    {
        /// <summary>
        /// ID del rol
        /// Foreign Key a Role
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// ID del permiso
        /// Foreign Key a Permission
        /// </summary>
        public int PermissionId { get; set; }

        /// <summary>
        /// Fecha de asignación del permiso
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID del usuario que asignó el permiso
        /// </summary>
        public int CreatedBy { get; set; }

        // Navigation properties
        public Role Role { get; set; }
        public Permission Permission { get; set; }

        public void Validate()
        {
            if (RoleId <= 0)
                throw new ArgumentException("RoleId debe ser válido");
            if (PermissionId <= 0)
                throw new ArgumentException("PermissionId debe ser válido");
        }
    }
}
```

### RefreshToken.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Token de renovación para JWT (Refresh Token)
    /// Auditable: No | SoftDelete: No
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        /// <summary>
        /// ID del usuario propietario del token
        /// Foreign Key a User
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Token hash (nunca guardar token en texto plano)
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Fecha de expiración del refresh token
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Indica si el token fue revocado
        /// </summary>
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// Dirección IP desde la cual se generó el token
        /// Para seguridad adicional (detectar tokens en IPs sospechosas)
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User Agent del cliente que generó el token
        /// </summary>
        public string UserAgent { get; set; }

        // Navigation property
        public User User { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;

        public void Validate()
        {
            if (UserId <= 0)
                throw new ArgumentException("UserId debe ser válido");
            if (string.IsNullOrWhiteSpace(Token))
                throw new ArgumentException("Token es requerido");
            if (ExpiryDate <= DateTime.UtcNow)
                throw new ArgumentException("ExpiryDate debe ser en el futuro");
        }
    }
}
```

---

## 3️⃣ ENTIDADES DE FORMULARIOS

### FormTemplate.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Plantilla reutilizable para formularios
    /// Auditable: Sí | SoftDelete: Sí
    /// </summary>
    public class FormTemplate : AuditableEntity
    {
        /// <summary>
        /// Nombre único de la plantilla (ej: "Solicitud de Crédito")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción detallada del propósito de la plantilla
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Versión de la plantilla (para versionamiento)
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Indica si la plantilla está activa para nuevos envíos
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        /// <summary>
        /// Campos que conforman esta plantilla
        /// </summary>
        public ICollection<FormField> Fields { get; set; } = new List<FormField>();

        /// <summary>
        /// Envíos de formularios usando esta plantilla
        /// </summary>
        public ICollection<FormSubmission> Submissions { get; set; } = new List<FormSubmission>();

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("El nombre de la plantilla es requerido");
            if (Fields?.Count == 0)
                throw new ArgumentException("La plantilla debe tener al menos un campo");
            if (Version < 1)
                throw new ArgumentException("La versión debe ser >= 1");
        }
    }
}
```

### FormField.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Campo individual en una plantilla de formulario
    /// Auditable: No | SoftDelete: No
    /// </summary>
    public class FormField : BaseEntity
    {
        /// <summary>
        /// ID de la plantilla a la que pertenece
        /// Foreign Key a FormTemplate
        /// </summary>
        public int FormTemplateId { get; set; }

        /// <summary>
        /// Nombre del campo (ej: "Nombre", "Email", "Edad")
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Tipo de campo (text, email, number, date, select, textarea, etc.)
        /// </summary>
        public string FieldType { get; set; }

        /// <summary>
        /// Indica si el campo es obligatorio
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// Orden de aparición en el formulario (para UI)
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Valor por defecto (opcional)
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Texto de ayuda para el usuario (placeholder)
        /// </summary>
        public string HelpText { get; set; }

        // Navigation property
        public FormTemplate FormTemplate { get; set; }

        /// <summary>
        /// Validaciones específicas del campo
        /// </summary>
        public ICollection<FormFieldValidation> Validations { get; set; } = 
            new List<FormFieldValidation>();

        public void Validate()
        {
            if (FormTemplateId <= 0)
                throw new ArgumentException("FormTemplateId debe ser válido");
            if (string.IsNullOrWhiteSpace(FieldName))
                throw new ArgumentException("El nombre del campo es requerido");
            if (string.IsNullOrWhiteSpace(FieldType))
                throw new ArgumentException("El tipo de campo es requerido");
        }
    }
}
```

### FormFieldValidation.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Regla de validación para un campo de formulario
    /// Auditable: No | SoftDelete: No
    /// </summary>
    public class FormFieldValidation : BaseEntity
    {
        /// <summary>
        /// ID del campo que se valida
        /// Foreign Key a FormField
        /// </summary>
        public int FormFieldId { get; set; }

        /// <summary>
        /// Tipo de validación (required, email, minLength, maxLength, pattern, min, max)
        /// </summary>
        public string ValidationRule { get; set; }

        /// <summary>
        /// Mensaje de error personalizado
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Valor mínimo (para validaciones numérico/longitud)
        /// </summary>
        public string MinValue { get; set; }

        /// <summary>
        /// Valor máximo (para validaciones numérico/longitud)
        /// </summary>
        public string MaxValue { get; set; }

        /// <summary>
        /// Patrón regex (para validaciones pattern)
        /// </summary>
        public string Pattern { get; set; }

        // Navigation property
        public FormField FormField { get; set; }

        public void Validate()
        {
            if (FormFieldId <= 0)
                throw new ArgumentException("FormFieldId debe ser válido");
            if (string.IsNullOrWhiteSpace(ValidationRule))
                throw new ArgumentException("La regla de validación es requerida");
        }
    }
}
```

### FormSubmissionHistory.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Historial de cambios en un envío de formulario
    /// Auditable: No | SoftDelete: No
    /// </summary>
    public class FormSubmissionHistory : BaseEntity
    {
        /// <summary>
        /// ID del formulario que se modificó
        /// Foreign Key a FormSubmission
        /// </summary>
        public int FormSubmissionId { get; set; }

        /// <summary>
        /// Estado anterior (ej: "Pendiente")
        /// </summary>
        public string OldStatus { get; set; }

        /// <summary>
        /// Nuevo estado (ej: "Aprobado")
        /// </summary>
        public string NewStatus { get; set; }

        /// <summary>
        /// ID del usuario que hizo el cambio
        /// </summary>
        public int ChangedBy { get; set; }

        /// <summary>
        /// Fecha del cambio
        /// </summary>
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Razón del cambio (comentario/motivo)
        /// </summary>
        public string Reason { get; set; }

        // Navigation property
        public FormSubmission FormSubmission { get; set; }

        public void Validate()
        {
            if (FormSubmissionId <= 0)
                throw new ArgumentException("FormSubmissionId debe ser válido");
            if (string.IsNullOrWhiteSpace(OldStatus))
                throw new ArgumentException("OldStatus es requerido");
            if (string.IsNullOrWhiteSpace(NewStatus))
                throw new ArgumentException("NewStatus es requerido");
        }
    }
}
```

---

## 4️⃣ ENTIDADES DE AUDITORÍA

### AuditLog.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Log de auditoría: Quién, cuándo, qué cambió
    /// Auditable: No | SoftDelete: No
    /// Uso: Trazabilidad completa de acciones en el sistema
    /// </summary>
    public class AuditLog : BaseEntity
    {
        /// <summary>
        /// ID del usuario que realizó la acción
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Acción realizada (CREATE, READ, UPDATE, DELETE, LOGIN, LOGOUT, EXPORT)
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Tipo de entidad afectada (ej: "FormSubmission", "User", "Role")
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// ID de la entidad afectada
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Valores anteriores en formato JSON
        /// Ejemplo: {"Status": "Pendiente", "ReviewedBy": null}
        /// </summary>
        public string OldValues { get; set; }

        /// <summary>
        /// Nuevos valores en formato JSON
        /// Ejemplo: {"Status": "Aprobado", "ReviewedBy": 1}
        /// </summary>
        public string NewValues { get; set; }

        /// <summary>
        /// Dirección IP del cliente
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User Agent del navegador/cliente
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Timestamp UTC de la acción
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User User { get; set; }

        public void Validate()
        {
            if (UserId <= 0)
                throw new ArgumentException("UserId debe ser válido");
            if (string.IsNullOrWhiteSpace(Action))
                throw new ArgumentException("Action es requerida");
            if (string.IsNullOrWhiteSpace(EntityType))
                throw new ArgumentException("EntityType es requerido");
        }
    }
}
```

---

## 5️⃣ ENTIDADES DE CONFIGURACIÓN

### AppSettings.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Configuración global de la aplicación
    /// Auditable: No | SoftDelete: No
    /// Uso: Almacenar settings dinámicos (sin redeploy)
    /// </summary>
    public class AppSettings : BaseEntity
    {
        /// <summary>
        /// Clave única del setting (ej: "MaxFormSize", "ExportTimeout", "TokenExpiry")
        /// </summary>
        public string SettingKey { get; set; }

        /// <summary>
        /// Valor del setting (puede ser JSON para configuraciones complejas)
        /// </summary>
        public string SettingValue { get; set; }

        /// <summary>
        /// Descripción del propósito del setting
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Tipo de dato del valor (string, int, bool, json)
        /// </summary>
        public string DataType { get; set; } = "string";

        /// <summary>
        /// Última modificación
        /// </summary>
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Usuario que hizo la última modificación
        /// </summary>
        public int LastModifiedBy { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SettingKey))
                throw new ArgumentException("SettingKey es requerido");
            if (string.IsNullOrWhiteSpace(SettingValue))
                throw new ArgumentException("SettingValue es requerido");
        }
    }
}
```

### UserPreferences.cs
```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// Preferencias de usuario (1:1 con User)
    /// Auditable: No | SoftDelete: No
    /// Uso: Guardar preferencias personalizadas por usuario
    /// </summary>
    public class UserPreferences : BaseEntity
    {
        /// <summary>
        /// ID del usuario (relación 1:1)
        /// Foreign Key a User
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Tema preferido (light, dark, auto)
        /// </summary>
        public string Theme { get; set; } = "light";

        /// <summary>
        /// Idioma preferido (es, en, fr, etc.)
        /// </summary>
        public string Language { get; set; } = "es";

        /// <summary>
        /// Recibir notificaciones por email
        /// </summary>
        public bool EmailNotifications { get; set; } = true;

        /// <summary>
        /// Recibir notificaciones por SMS
        /// </summary>
        public bool SmsNotifications { get; set; } = false;

        /// <summary>
        /// Zona horaria del usuario (ej: "America/Bogota")
        /// </summary>
        public string TimeZone { get; set; } = "UTC";

        /// <summary>
        /// Última actualización de preferencias
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User User { get; set; }

        public void Validate()
        {
            if (UserId <= 0)
                throw new ArgumentException("UserId debe ser válido");
            if (string.IsNullOrEmpty(Language))
                throw new ArgumentException("Language es requerido");
        }
    }
}
```

---

## 6️⃣ ACTUALIZAR ENTIDADES EXISTENTES

### User.cs (ACTUALIZAR)
```csharp
namespace AutoCheckAML.Api.Entity
{
    // CAMBIO: Heredar de AuditableEntity en lugar de nada
    public class User : AuditableEntity  // ← CAMBIO AQUÍ
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLogin { get; set; }

        // Navigation
        public ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
        
        // NUEVAS NAVEGACIONES:
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public UserPreferences Preferences { get; set; }
    }
}
```

### FormSubmission.cs (ACTUALIZAR)
```csharp
namespace AutoCheckAML.Api.Entity
{
    // CAMBIO: Heredar de AuditableEntity en lugar de nada
    public class FormSubmission : AuditableEntity  // ← CAMBIO AQUÍ
    {
        public int UserId { get; set; }
        public int FormTemplateId { get; set; }  // ← NUEVO
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Empresa { get; set; }
        public string Asunto { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
        // Eliminado: public DateTime CreatedAt { get; set; }  (hereda de AuditableEntity)
        public string Status { get; set; } = "Pendiente";
        
        // NUEVOS CAMPOS:
        public string FormDataJson { get; set; }  // Datos completos en JSON
        public int? ReviewedBy { get; set; }      // Usuario que revisó
        public DateTime? ReviewedAt { get; set; }  // Fecha de revisión
        public string ReviewComments { get; set; } // Comentarios de revisión

        // Foreign Key
        public User User { get; set; }
        
        // NUEVAS NAVEGACIONES:
        public FormTemplate FormTemplate { get; set; }
        public ICollection<FormSubmissionHistory> History { get; set; } = new List<FormSubmissionHistory>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
```

---

## 7️⃣ ACTUALIZAR DbContext

### AutoCheckAMLContext.cs (AGREGAR)
```csharp
using AutoCheckAML.Api.Entity;
using Microsoft.EntityFrameworkCore;

namespace AutoCheckAML.Api.Data
{
    public class AutoCheckAMLContext : DbContext
    {
        public AutoCheckAMLContext(DbContextOptions<AutoCheckAMLContext> options) : base(options)
        {
        }

        // ENTIDADES EXISTENTES
        public DbSet<User> Users { get; set; }
        public DbSet<FormSubmission> FormSubmissions { get; set; }

        // NUEVAS ENTIDADES
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermissionMapping> RolePermissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        
        public DbSet<FormTemplate> FormTemplates { get; set; }
        public DbSet<FormField> FormFields { get; set; }
        public DbSet<FormFieldValidation> FormFieldValidations { get; set; }
        public DbSet<FormSubmissionHistory> FormSubmissionHistories { get; set; }
        
        public DbSet<AuditLog> AuditLogs { get; set; }
        
        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============ SOFT DELETE QUERY FILTERS ============
            
            // User: Solo activos (no eliminados)
            modelBuilder.Entity<User>()
                .HasQueryFilter(u => !u.IsDeleted);

            // FormSubmission: Solo activos
            modelBuilder.Entity<FormSubmission>()
                .HasQueryFilter(f => !f.IsDeleted);

            // Role: Solo activos
            modelBuilder.Entity<Role>()
                .HasQueryFilter(r => !r.IsDeleted);

            // FormTemplate: Solo activos
            modelBuilder.Entity<FormTemplate>()
                .HasQueryFilter(ft => !ft.IsDeleted);

            // ============ FOREIGN KEY RELATIONSHIPS ============

            // User -> FormSubmissions (1:N)
            modelBuilder.Entity<FormSubmission>()
                .HasOne(f => f.User)
                .WithMany(u => u.FormSubmissions)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> UserRoles (1:N)
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Role -> UserRoles (1:N)
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Role -> RolePermissions (1:N)
            modelBuilder.Entity<RolePermissionMapping>()
                .HasOne(rpm => rpm.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rpm => rpm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Permission -> RolePermissions (1:N)
            modelBuilder.Entity<RolePermissionMapping>()
                .HasOne(rpm => rpm.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rpm => rpm.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> RefreshTokens (1:N)
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // FormTemplate -> FormSubmissions (1:N)
            modelBuilder.Entity<FormSubmission>()
                .HasOne(f => f.FormTemplate)
                .WithMany(ft => ft.Submissions)
                .HasForeignKey(f => f.FormTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormTemplate -> FormFields (1:N)
            modelBuilder.Entity<FormField>()
                .HasOne(ff => ff.FormTemplate)
                .WithMany(ft => ft.Fields)
                .HasForeignKey(ff => ff.FormTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // FormField -> FormFieldValidations (1:N)
            modelBuilder.Entity<FormFieldValidation>()
                .HasOne(ffv => ffv.FormField)
                .WithMany(ff => ff.Validations)
                .HasForeignKey(ffv => ffv.FormFieldId)
                .OnDelete(DeleteBehavior.Cascade);

            // FormSubmission -> FormSubmissionHistories (1:N)
            modelBuilder.Entity<FormSubmissionHistory>()
                .HasOne(fsh => fsh.FormSubmission)
                .WithMany(fs => fs.History)
                .HasForeignKey(fsh => fsh.FormSubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> AuditLogs (1:N)
            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> UserPreferences (1:1)
            modelBuilder.Entity<UserPreferences>()
                .HasOne(up => up.User)
                .WithOne(u => u.Preferences)
                .HasForeignKey<UserPreferences>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============ INDICES ============

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique().HasDatabaseName("IX_User_Username");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique().HasDatabaseName("IX_User_Email");

            // Role
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name).IsUnique().HasDatabaseName("IX_Role_Name");

            // Permission
            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.Code).IsUnique().HasDatabaseName("IX_Permission_Code");

            // UserRole (compuesto)
            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique().HasDatabaseName("IX_UserRole_User_Role");

            // RolePermission (compuesto)
            modelBuilder.Entity<RolePermissionMapping>()
                .HasIndex(rpm => new { rpm.RoleId, rpm.PermissionId })
                .IsUnique().HasDatabaseName("IX_RolePermission_Role_Permission");

            // RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token).IsUnique().HasDatabaseName("IX_RefreshToken_Token");

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.ExpiryDate).HasDatabaseName("IX_RefreshToken_Expiry");

            // FormSubmission (compuesto para búsqueda rápida)
            modelBuilder.Entity<FormSubmission>()
                .HasIndex(f => new { f.UserId, f.Status, f.CreatedAt })
                .HasDatabaseName("IX_FormSubmission_User_Status_Date");

            // AuditLog (compuesto)
            modelBuilder.Entity<AuditLog>()
                .HasIndex(al => new { al.EntityType, al.EntityId, al.Timestamp })
                .HasDatabaseName("IX_AuditLog_Entity_Timestamp");

            modelBuilder.Entity<AuditLog>()
                .HasIndex(al => al.UserId).HasDatabaseName("IX_AuditLog_User");

            // UserPreferences
            modelBuilder.Entity<UserPreferences>()
                .HasIndex(up => up.UserId).IsUnique().HasDatabaseName("IX_UserPreferences_User");

            // ============ SEED DATA ============

            // Permissions por defecto
            var permissions = new Permission[]
            {
                // Form Permissions
                new Permission { Id = 1, Code = "FORM_CREATE", Name = "Crear Formulario", Resource = "Form", Action = "CREATE", Description = "Permite enviar nuevos formularios" },
                new Permission { Id = 2, Code = "FORM_READ", Name = "Leer Formulario", Resource = "Form", Action = "READ", Description = "Permite ver formularios" },
                new Permission { Id = 3, Code = "FORM_UPDATE", Name = "Actualizar Formulario", Resource = "Form", Action = "UPDATE", Description = "Permite editar formularios" },
                new Permission { Id = 4, Code = "FORM_DELETE", Name = "Eliminar Formulario", Resource = "Form", Action = "DELETE", Description = "Permite eliminar formularios" },
                new Permission { Id = 5, Code = "FORM_EXPORT", Name = "Exportar Formularios", Resource = "Form", Action = "EXPORT", Description = "Permite exportar a Excel" },
                new Permission { Id = 6, Code = "FORM_APPROVE", Name = "Aprobar Formulario", Resource = "Form", Action = "APPROVE", Description = "Permite aprobar formularios" },
                new Permission { Id = 7, Code = "FORM_REJECT", Name = "Rechazar Formulario", Resource = "Form", Action = "REJECT", Description = "Permite rechazar formularios" },
                
                // Role Permissions
                new Permission { Id = 8, Code = "ROLE_CREATE", Name = "Crear Rol", Resource = "Role", Action = "CREATE", Description = "Permite crear nuevos roles" },
                new Permission { Id = 9, Code = "ROLE_READ", Name = "Leer Rol", Resource = "Role", Action = "READ", Description = "Permite ver roles" },
                new Permission { Id = 10, Code = "ROLE_UPDATE", Name = "Actualizar Rol", Resource = "Role", Action = "UPDATE", Description = "Permite editar roles" },
                new Permission { Id = 11, Code = "ROLE_DELETE", Name = "Eliminar Rol", Resource = "Role", Action = "DELETE", Description = "Permite eliminar roles" },
                
                // User Permissions
                new Permission { Id = 12, Code = "USER_CREATE", Name = "Crear Usuario", Resource = "User", Action = "CREATE", Description = "Permite crear usuarios" },
                new Permission { Id = 13, Code = "USER_READ", Name = "Leer Usuario", Resource = "User", Action = "READ", Description = "Permite ver usuarios" },
                new Permission { Id = 14, Code = "USER_UPDATE", Name = "Actualizar Usuario", Resource = "User", Action = "UPDATE", Description = "Permite editar usuarios" },
                new Permission { Id = 15, Code = "USER_DELETE", Name = "Eliminar Usuario", Resource = "User", Action = "DELETE", Description = "Permite eliminar usuarios" },
                
                // Audit Permissions
                new Permission { Id = 16, Code = "AUDIT_VIEW", Name = "Ver Auditoría", Resource = "Audit", Action = "READ", Description = "Permite ver logs de auditoría" }
            };

            modelBuilder.Entity<Permission>().HasData(permissions);

            // Roles por defecto
            var roles = new Role[]
            {
                new Role { Id = 1, Name = "Admin", Description = "Administrador del sistema con acceso total", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Role { Id = 2, Name = "Manager", Description = "Gerente que aprueba formularios", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Role { Id = 3, Name = "User", Description = "Usuario normal que envía formularios", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            modelBuilder.Entity<Role>().HasData(roles);

            // Role -> Permission Mappings
            var rolePermissions = new RolePermissionMapping[]
            {
                // Admin tiene todos los permisos
                new RolePermissionMapping { Id = 1, RoleId = 1, PermissionId = 1, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 2, RoleId = 1, PermissionId = 2, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 3, RoleId = 1, PermissionId = 3, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 4, RoleId = 1, PermissionId = 4, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 5, RoleId = 1, PermissionId = 5, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 6, RoleId = 1, PermissionId = 6, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 7, RoleId = 1, PermissionId = 7, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 8, RoleId = 1, PermissionId = 8, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 9, RoleId = 1, PermissionId = 9, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 10, RoleId = 1, PermissionId = 10, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 11, RoleId = 1, PermissionId = 11, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 12, RoleId = 1, PermissionId = 12, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 13, RoleId = 1, PermissionId = 13, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 14, RoleId = 1, PermissionId = 14, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 15, RoleId = 1, PermissionId = 15, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 16, RoleId = 1, PermissionId = 16, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },

                // Manager puede leer/aprobar/rechazar/exportar formularios
                new RolePermissionMapping { Id = 17, RoleId = 2, PermissionId = 2, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 18, RoleId = 2, PermissionId = 5, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 19, RoleId = 2, PermissionId = 6, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 20, RoleId = 2, PermissionId = 7, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },

                // User puede crear/leer/actualizar sus propios formularios
                new RolePermissionMapping { Id = 21, RoleId = 3, PermissionId = 1, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 22, RoleId = 3, PermissionId = 2, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new RolePermissionMapping { Id = 23, RoleId = 3, PermissionId = 3, CreatedAt = DateTime.UtcNow, CreatedBy = 1 }
            };

            modelBuilder.Entity<RolePermissionMapping>().HasData(rolePermissions);

            // Usuario admin por defecto
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@autocheck.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FullName = "Administrador",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    IsDeleted = false
                }
            );

            // Asignar rol Admin al usuario admin
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    Id = 1,
                    UserId = 1,
                    RoleId = 1,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = 1
                }
            );
        }
    }
}
```

---

## ✅ CHECKLIST: Pasos para Implementar

```
1. CREAR ARCHIVOS (En Entity/)
   ☐ BaseEntity.cs
   ☐ AuditableEntity.cs
   ☐ Role.cs
   ☐ Permission.cs
   ☐ UserRole.cs
   ☐ RolePermissionMapping.cs
   ☐ RefreshToken.cs
   ☐ FormTemplate.cs
   ☐ FormField.cs
   ☐ FormFieldValidation.cs
   ☐ FormSubmissionHistory.cs
   ☐ AuditLog.cs
   ☐ AppSettings.cs
   ☐ UserPreferences.cs

2. ACTUALIZAR ARCHIVOS
   ☐ User.cs (cambiar herencia)
   ☐ FormSubmission.cs (cambiar herencia)

3. ACTUALIZAR CONTEXTO
   ☐ AutoCheckAMLContext.cs (DbSets + OnModelCreating)

4. EF MIGRATIONS
   ☐ dotnet ef migrations add AddRBACEntities
   ☐ dotnet ef database update

5. COMPILAR Y TESTEAR
   ☐ dotnet build (debe compilar sin errores)
   ☐ Verificar BD fue creada correctamente
   ☐ Verificar permisos y roles en seed data
```

---

**Código Generado:** 2 Junio, 2026  
**Estado:** Listo para Copiar/Pegar  
**Testing:** Verificar después de migración
