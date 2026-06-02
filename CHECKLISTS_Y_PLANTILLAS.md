# 📋 CHECKLISTS EJECUTABLES Y PLANTILLAS DE IMPLEMENTACIÓN
## AutoCheckAML - Guía Práctica

---

## CHECKLIST FASE 1: PLANIFICACIÓN (Semana 1)

### Diagramas y Diseño
- [ ] **MER Completo** 
  - [ ] Identificar 16 entidades
  - [ ] Definir relaciones y cardinalidades (1:1, 1:N, N:N)
  - [ ] Marcar campos auditables vs no auditables
  - [ ] Documentar índices necesarios
  - **Entregable:** `MER_DIAGRAM.mermaid`

- [ ] **UML Diagrama de Clases**
  - [ ] Definir jerarquía: BaseEntity → AuditableEntity
  - [ ] Mostrar interfaces (IRepository, ISpecification)
  - [ ] Relaciones entre entidades
  - **Entregable:** `UML_CLASS_DIAGRAM.mermaid`

- [ ] **Diagramas de Flujo**
  - [ ] Flujo RBAC (autenticación → autorización)
  - [ ] Flujo exportación a Excel
  - [ ] Máquina de estados de formulario
  - [ ] Flujo refresh token
  - **Entregable:** 4 archivos `.mermaid`

### Análisis de Requisitos
- [ ] **Casos de Uso** (6-10 principales)
  - [ ] UC001: Admin gestiona roles/permisos
  - [ ] UC002: Usuario envía formulario
  - [ ] UC003: Manager aprueba/rechaza
  - [ ] UC004: Admin exporta a Excel
  - [ ] UC005: Usuario renueva token JWT
  - [ ] UC006: Sistema archiva formularios
  - **Entregable:** `CASOS_DE_USO.md` (Cockburn format)

- [ ] **Historias de Usuario** (BDD)
  - [ ] 15+ historias en formato Gherkin
  - [ ] Acceptance criteria definido
  - [ ] Estimación Story Points (fibonacci)
  - **Entregable:** `USER_STORIES.md`

### Especificación de Transiciones
- [ ] **Estados de Formulario**
  - [ ] Pendiente → EnRevision → Aprobado → Completado → Archivado
  - [ ] Estados alternativos: Rechazado, Cancelado
  - [ ] Tabla de transiciones permitidas
  - [ ] Actores autorizados por transición
  - **Entregable:** `FORM_STATE_MACHINE.md` + diagrama

- [ ] **Permisos Base**
  - [ ] FORM_CREATE, FORM_READ, FORM_UPDATE, FORM_DELETE
  - [ ] FORM_EXPORT, FORM_APPROVE, FORM_REJECT
  - [ ] ROLE_CREATE, ROLE_DELETE, ROLE_MANAGE
  - [ ] USER_CREATE, USER_DELETE, USER_MANAGE
  - [ ] AUDIT_VIEW, AUDIT_DELETE
  - **Entregable:** `PERMISSIONS_CATALOG.md`

### Seguridad Inicial
- [ ] **JWT Setup**
  - [ ] Definir duración access token (15 min)
  - [ ] Definir duración refresh token (7 días)
  - [ ] Claims a incluir (userId, username, roles)
  - **Entregable:** `JWT_SPECIFICATION.md`

- [ ] **CORS Policy**
  - [ ] Definir orígenes permitidos (desarrollo, staging, producción)
  - [ ] Headers expuestos (X-Total-Count, etc.)
  - [ ] Métodos permitidos
  - **Entregable:** `CORS_CONFIGURATION.md`

### Índices de BD
- [ ] Planificar índices críticos
  - [ ] User: (Username), (Email)
  - [ ] FormSubmission: (UserId, Status, CreatedAt)
  - [ ] AuditLog: (EntityType, EntityId, Timestamp)
  - [ ] RefreshToken: (Token), (ExpiryDate)
  - **Entregable:** `DATABASE_INDEXES.md`

---

## CHECKLIST FASE 2: ARQUITECTURA (Semana 2-3)

### Base Classes
- [ ] **BaseEntity.cs**
  ```csharp
  public abstract class BaseEntity
  {
      public int Id { get; set; }
      public DateTime CreatedAt { get; set; }
  }
  ```

- [ ] **AuditableEntity.cs**
  ```csharp
  public abstract class AuditableEntity : BaseEntity
  {
      public DateTime? UpdatedAt { get; set; }
      public int? UpdatedBy { get; set; }
      public DateTime? DeletedAt { get; set; }
      public int? DeletedBy { get; set; }
      public bool IsDeleted { get; set; }
  }
  ```

- [ ] **StatusEntity.cs** (Opcional)
  ```csharp
  public abstract class StatusEntity : AuditableEntity
  {
      public string Status { get; set; }
      public string StatusReason { get; set; }
  }
  ```

### Patrones Avanzados
- [ ] **Specification Pattern**
  - [ ] BaseSpecification<T>
  - [ ] ISpecificationEvaluator<T>
  - [ ] Ejemplo: PermittedFormsSpecification
  - **Entregable:** `Specifications/BaseSpecification.cs` + 5 ejemplos

- [ ] **Extended Unit of Work**
  - [ ] Agregar repositorios faltantes
  - [ ] Método SaveChangesAsync mejorado
  - [ ] Método SoftDeleteAsync
  - **Entregable:** Actualizar `IUnitOfWork.cs`

- [ ] **Authorization Service**
  - [ ] HasPermissionAsync()
  - [ ] HasAllPermissionsAsync()
  - [ ] HasAnyPermissionAsync()
  - [ ] GetUserPermissionsAsync() con caché
  - **Entregable:** `Business/IAuthorizationService.cs`

### Auditoría
- [ ] **Audit Service**
  - [ ] LogCreateAsync()
  - [ ] LogUpdateAsync()
  - [ ] LogDeleteAsync()
  - [ ] LogAccessAsync()
  - [ ] GetAuditLogsAsync()
  - **Entregable:** `Business/IAuditService.cs`

### Documentación Técnica
- [ ] `ARCHITECTURE_DECISION_RECORDS.md` (3-5 ADRs)
- [ ] `DESIGN_PATTERNS_USED.md`
- [ ] `DEPENDENCY_INJECTION.md`

---

## CHECKLIST FASE 3: ENTIDADES (Semana 3-4)

### Crear Entidades (16 Total)

#### Bloque Seguridad (6 entidades)
- [ ] **User.cs** (actualizar a AuditableEntity)
  - Username, Email, PasswordHash, FullName
  - IsActive, LastLogin
  - Navegación: UserRoles, FormSubmissions, AuditLogs

- [ ] **Role.cs** (AuditableEntity)
  - Name (único), Description, IsActive
  - Navegación: UserRoles, RolePermissionMappings

- [ ] **Permission.cs** (BaseEntity)
  - Code (único), Name, Description
  - Resource, Action
  - Navegación: RolePermissionMappings

- [ ] **UserRole.cs** (BaseEntity)
  - UserId, RoleId (PK compuesta)
  - AssignedAt, AssignedBy
  - RevokedAt, RevokedBy
  - Navegación: User, Role

- [ ] **RolePermissionMapping.cs** (BaseEntity)
  - RoleId, PermissionId (PK compuesta)
  - CreatedAt, CreatedBy
  - Navegación: Role, Permission

- [ ] **RefreshToken.cs** (BaseEntity)
  - UserId, Token (único)
  - ExpiryDate, IsRevoked
  - IpAddress, UserAgent
  - Navegación: User

#### Bloque Formularios (6 entidades)
- [ ] **FormTemplate.cs** (AuditableEntity)
  - Name, Description, Version
  - IsActive
  - Navegación: FormFields, FormSubmissions

- [ ] **FormField.cs** (BaseEntity)
  - FormTemplateId, FieldName, FieldType
  - IsRequired, DisplayOrder, DefaultValue
  - Navegación: FormTemplate, Validations

- [ ] **FormFieldValidation.cs** (BaseEntity)
  - FormFieldId, ValidationRule
  - ErrorMessage, MinValue, MaxValue
  - Navegación: FormField

- [ ] **FormSubmission.cs** (actualizar a AuditableEntity)
  - UserId, FormTemplateId
  - Status, FormDataJson
  - SubmissionDate, ReviewedBy, ReviewedAt, ReviewComments
  - Navegación: User, FormTemplate, History, AuditLogs

- [ ] **FormSubmissionHistory.cs** (BaseEntity)
  - FormSubmissionId, OldStatus, NewStatus
  - ChangedBy, ChangedAt, Reason
  - Navegación: FormSubmission

#### Bloque Auditoría (1 entidad)
- [ ] **AuditLog.cs** (BaseEntity)
  - UserId, Action, EntityType, EntityId
  - OldValues (JSON), NewValues (JSON)
  - IpAddress, UserAgent, Timestamp
  - Navegación: User

#### Bloque Configuración (3 entidades)
- [ ] **AppSettings.cs** (BaseEntity)
  - SettingKey (único), SettingValue
  - Description, LastModifiedAt, LastModifiedBy

- [ ] **UserPreferences.cs** (BaseEntity)
  - UserId (único), Theme, Language
  - EmailNotifications, SmsNotifications
  - TimeZone, UpdatedAt

- [ ] **Notification.cs** (AuditableEntity) - FUTURO
  - UserId, Type, Title, Message
  - IsRead, ReadAt
  - Navegación: User

### DbContext Configuration
- [ ] Configurar todas las entidades en OnModelCreating()
  - [ ] Foreign keys con DeleteBehavior
  - [ ] Índices (incluidos compuestos)
  - [ ] Seed data inicial
  - [ ] Query filters para SoftDelete

**Entregable:** 
- 16 archivos `.cs` en Entity/
- `AutoCheckAMLContext.cs` actualizado
- `20260602_InitialMigration.cs` (EF Migration)

---

## CHECKLIST FASE 4: SERVICIOS (Semana 4-5)

### Core Services
- [ ] **IAuthorizationService**
  - Implementación con caché Redis
  - Unit tests (>90% coverage)
  
- [ ] **IAuditService**
  - Métodos para Create, Update, Delete, Access
  - Serialización JSON de valores old/new
  - IP y UserAgent capture

- [ ] **ITokenService** (mejorado)
  - GenerateAccessToken()
  - GenerateRefreshToken()
  - RefreshAccessTokenAsync()
  - RevokeRefreshTokenAsync()

- [ ] **IPermissionService**
  - GetAllPermissionsAsync()
  - GetRolePermissionsAsync()
  - AssignPermissionToRoleAsync()
  - RemovePermissionFromRoleAsync()

- [ ] **ISpecificationEvaluator**
  - GetQuery() - aplica specification a IQueryable
  - Incluye: Criteria, Includes, Paging, Ordering

### Middleware Actualizado
- [ ] **ExceptionHandlingMiddleware** (mejorado)
  - Captura IP del usuario
  - Registra en AuditLog accesos denegados
  - Respuesta normalizada para todos los errores

- [ ] **AuthenticationMiddleware** (nuevo)
  - Valida JWT
  - Extrae claims
  - Captura UserAgent

### Extension Methods
- [ ] `ClaimsPrincipalExtensions.GetUserId()`
- [ ] `ClaimsPrincipalExtensions.GetRoles()`
- [ ] `QueryableExtensions.ApplySpecification<T>()`
- [ ] `IQueryableExtensions.AsNoTracking()`

**Entregable:**
- 8+ servicios implementados
- Unit tests (Test/Services/)
- Integration tests (Test/Integration/)

---

## CHECKLIST FASE 5: API ENDPOINTS v2.0 (Semana 5-6)

### Versionamiento
- [ ] Instalar Microsoft.AspNetCore.ApiVersioning
- [ ] Configurar URL-based versioning (/api/v2/...)
- [ ] Deprecation notice en v1.0
- [ ] Swagger separado por versión

### Controllers v2.0

#### Auth Controller
- [ ] POST /api/v2/auth/login
- [ ] POST /api/v2/auth/register
- [ ] POST /api/v2/auth/refresh
- [ ] POST /api/v2/auth/logout
- [ ] POST /api/v2/auth/revoke-refresh-token

#### Forms Controller
- [ ] GET /api/v2/forms (con paginación)
- [ ] GET /api/v2/forms/{id}
- [ ] POST /api/v2/forms
- [ ] PUT /api/v2/forms/{id}
- [ ] DELETE /api/v2/forms/{id}
- [ ] POST /api/v2/forms/{id}/status
- [ ] POST /api/v2/forms/search
- [ ] POST /api/v2/forms/export

#### Roles & Permissions Controller (NUEVO)
- [ ] GET /api/v2/roles
- [ ] GET /api/v2/roles/{id}
- [ ] POST /api/v2/roles [Admin only]
- [ ] PUT /api/v2/roles/{id} [Admin only]
- [ ] DELETE /api/v2/roles/{id} [Admin only]
- [ ] GET /api/v2/permissions
- [ ] POST /api/v2/roles/{roleId}/permissions/{permissionId} [Admin]
- [ ] DELETE /api/v2/roles/{roleId}/permissions/{permissionId} [Admin]

#### FormTemplates Controller (NUEVO)
- [ ] GET /api/v2/form-templates
- [ ] GET /api/v2/form-templates/{id}
- [ ] POST /api/v2/form-templates [Manager+]
- [ ] PUT /api/v2/form-templates/{id} [Manager+]
- [ ] DELETE /api/v2/form-templates/{id} [Admin]

#### Audit Controller (NUEVO)
- [ ] GET /api/v2/audit-logs [Admin]
- [ ] GET /api/v2/audit-logs/{entityType}/{entityId} [Own data]
- [ ] GET /api/v2/audit-logs/by-user/{userId} [Admin]
- [ ] GET /api/v2/audit-logs/export [Admin]

### OpenAPI/Swagger
- [ ] Documentación en cada endpoint (/// <summary>)
- [ ] ProducesResponseType (200, 400, 401, 403, 404, 500)
- [ ] Request/Response examples
- [ ] Security requirement: Bearer JWT

**Entregable:**
- 25+ endpoints documentados
- Swagger JSON actualizado
- Postman collection
- `API_CHANGES_V1_TO_V2.md`

---

## CHECKLIST FASE 6: TESTING (Semana 6-7)

### Unit Tests

#### Repositories
- [ ] RepositoryTests.cs
  - GetByIdAsync - found/not found
  - GetAllAsync - con/sin deleted
  - FindAsync - con filtros
  - AddAsync, UpdateAsync, DeleteAsync
  - SoftDeleteAsync
  - **Target:** >95% coverage

#### Services
- [ ] AuthorizationServiceTests.cs
  - HasPermissionAsync - true/false
  - HasAllPermissionsAsync, HasAnyPermissionAsync
  - Cache testing
  - **Target:** >90% coverage

- [ ] AuditServiceTests.cs
  - LogCreateAsync, LogUpdateAsync, LogDeleteAsync
  - Serialización JSON
  - **Target:** >90% coverage

- [ ] TokenServiceTests.cs
  - GenerateAccessToken - claims correctos
  - GenerateRefreshToken - expiración
  - RefreshAccessTokenAsync - casos válidos/inválidos

#### Specifications
- [ ] PermittedFormsSpecificationTests.cs
  - Admin ve todos
  - Manager ve todos menos suyos
  - User ve solo suyos
  - **Target:** >95% coverage

### Integration Tests

#### Endpoints de Autenticación
- [ ] Login exitoso - retorna tokens
- [ ] Login fallido - 401
- [ ] Refresh token válido - nuevo access token
- [ ] Refresh token expirado - 401
- [ ] Request sin token - 401
- [ ] Request con token expirado - 401 (sin refresh)

#### Endpoints de Formularios
- [ ] User solo ve sus formularios
- [ ] Manager ve todos excepto suyos
- [ ] Admin ve todos incluidos soft-deleted
- [ ] Acceso denegado retorna 403
- [ ] Transición inválida retorna 400
- [ ] Cambio registrado en AuditLog

#### Endpoints de Exportación
- [ ] Export exitoso genera Excel
- [ ] Sin permiso EXPORT retorna 403
- [ ] Registra en AuditLog acción
- [ ] Incluye solo formularios permitidos

### Performance Tests
- [ ] Carga con 10,000 registros
- [ ] Query time < 200ms
- [ ] Caché reduce queries en 90%

### Security Tests
- [ ] SQL Injection - no vulnerable
- [ ] CSRF Protection - middleware activo
- [ ] XSS - responses sanitizadas
- [ ] Authorization bypass - no posible

**Entregable:**
- 50+ test cases
- Coverage >80% global
- GitHub Actions CI/CD
- Test report

---

## CHECKLIST FASE 7: DOCUMENTACIÓN FINAL (Semana 7-8)

### Documentación de Desarrollo
- [ ] **README.md** (actualizado)
  - Descripción del proyecto
  - Características principales
  - Stack tecnológico
  - Quick start

- [ ] **SETUP_LOCAL.md**
  - Requisitos previos
  - Clonar repositorio
  - Restaurar dependencias
  - Crear BD (migrations)
  - Ejecutar seed data
  - Configurar appsettings
  - Iniciar servidor

- [ ] **ARCHITECTURE.md**
  - Clean Architecture diagram
  - Carpetas y responsabilidades
  - Flujo de datos
  - Patrones implementados

- [ ] **DEVELOPMENT_GUIDE.md**
  - Convenciones de código
  - Naming conventions
  - Cómo agregar una entidad
  - Cómo crear un endpoint
  - Cómo escribir un test

### Documentación de Operaciones
- [ ] **DEPLOYMENT_GUIDE.md**
  - Producción checklist
  - Configuración SSL/HTTPS
  - Variables de entorno
  - Database migration en prod
  - Rollback procedures

- [ ] **TROUBLESHOOTING.md**
  - Errores comunes y soluciones
  - Debug mode
  - Logs y dónde encontrarlos
  - Performance issues
  - Security issues

- [ ] **API_REFERENCE.md**
  - Todos los endpoints
  - Request/Response ejemplos
  - Error codes y meanings
  - Rate limits
  - Deprecation policy

### Documentación de Negocio
- [ ] **USER_GUIDE.md**
  - Cómo usar la aplicación
  - Flujos principales
  - FAQs

- [ ] **CHANGE_LOG.md**
  - v1.0 → v2.0 cambios
  - Breaking changes
  - Migration guide

### Decisiones Arquitectónicas
- [ ] **ADR_LOG.md**
  - ADR-001: RBAC approach
  - ADR-002: Specification Pattern
  - ADR-003: Audit logging
  - ADR-004: API versioning
  - ADR-005: Caché strategy

**Entregable:**
- 10+ documentos markdown
- OpenAPI Swagger UI
- Todos los diagramas en Mermaid
- Wiki del proyecto

---

## PLANTILLAS DE CÓDIGO PRONTAS PARA COPIAR

### Plantilla 1: Nueva Entidad Auditable

```csharp
namespace AutoCheckAML.Api.Entity
{
    /// <summary>
    /// [REEMPLAZAR: Descripción de la entidad]
    /// Auditable: Sí | SoftDelete: Sí
    /// </summary>
    public class [ENTITY_NAME] : AuditableEntity
    {
        /// <summary>
        /// [REEMPLAZAR: Descripción del campo]
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Fecha de última modificación
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        // public ICollection<[RELATED_ENTITY]> [RelatedEntities] { get; set; } = new List<[RELATED_ENTITY]>();

        /// <summary>
        /// Validaciones de negocio
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(PropertyName))
                throw new ArgumentException("PropertyName es requerido");
            
            // Agregar más validaciones
        }
    }
}
```

### Plantilla 2: Specification Pattern

```csharp
using AutoCheckAML.Api.Entity;
using System.Linq.Expressions;

namespace AutoCheckAML.Api.Specifications
{
    /// <summary>
    /// [REEMPLAZAR: Descripción del filtro]
    /// Uso: var spec = new [SpecName](...);
    ///      var entities = await _repo.FindAsync(spec.Criteria);
    /// </summary>
    public class [SpecName] : Specification<[EntityName]>
    {
        public [SpecName]([PARAMETERS])
        {
            // Definir criterios
            Criteria = e => [FILTER_LOGIC];

            // Incluir relaciones si es necesario
            // AddInclude(e => e.RelatedEntity);

            // Ordenamiento (opcional)
            // OrderByDescending = e => e.CreatedAt;

            // Paginación (opcional)
            // ApplyPaging(0, 10);
        }
    }
}
```

### Plantilla 3: Service Interface

```csharp
using AutoCheckAML.Api.Entity;

namespace AutoCheckAML.Api.Business
{
    /// <summary>
    /// [REEMPLAZAR: Descripción del servicio]
    /// </summary>
    public interface I[ServiceName]
    {
        /// <summary>
        /// [REEMPLAZAR: Descripción del método]
        /// </summary>
        /// <param name="[param]">[Descripción]</param>
        /// <returns>[Descripción del retorno]</returns>
        /// <exception cref="ArgumentException">Si algún parámetro es inválido</exception>
        Task<[ReturnType]> [MethodName]([PARAMETERS]);

        // Más métodos...
    }

    public class [ServiceName] : I[ServiceName]
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<[ServiceName]> _logger;

        public [ServiceName](IUnitOfWork unitOfWork, ILogger<[ServiceName]> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<[ReturnType]> [MethodName]([PARAMETERS])
        {
            try
            {
                // Validación
                if ([VALIDATION_FAILED])
                    throw new ArgumentException("Mensaje de error");

                // Lógica
                var result = await _unitOfWork.[Repository].GetByIdAsync([param]);
                
                if (result == null)
                    throw new KeyNotFoundException("Entidad no encontrada");

                // Actualizar/Procesar
                // ...

                // Auditar
                // await _auditService.LogAsync(...);

                // Guardar
                await _unitOfWork.CommitAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en [MethodName]: {ex.Message}");
                throw;
            }
        }
    }
}
```

### Plantilla 4: Controller Endpoint (v2.0)

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoCheckAML.Api.Web.Controllers
{
    /// <summary>
    /// [REEMPLAZAR: Descripción del controlador]
    /// Base: /api/v{version}/[controller]
    /// Seguridad: JWT Bearer + RBAC
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [Authorize]
    public class [ControllerName] : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthorizationService _authService;
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;
        private readonly ILogger<[ControllerName]> _logger;

        public [ControllerName](
            IUnitOfWork unitOfWork,
            IAuthorizationService authService,
            IAuditService auditService,
            IMapper mapper,
            ILogger<[ControllerName]> logger)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _auditService = auditService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// [REEMPLAZAR: Descripción del endpoint]
        /// </summary>
        /// <remarks>
        /// **Permisos Requeridos:** [PERMISSION_CODE]
        /// **Rate Limit:** 100/min
        /// </remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<[DtoName]>> Get[EntityName](int id)
        {
            try
            {
                var userId = GetUserId();

                // Validar permiso
                if (!await _authService.HasPermissionAsync(userId, "[PERMISSION]"))
                {
                    _logger.LogWarning($"Acceso denegado para usuario {userId}");
                    return Forbid();
                }

                // Obtener entidad
                var entity = await _unitOfWork.[Repository].GetByIdAsync(id);
                if (entity == null)
                    return NotFound();

                // Auditar lectura
                await _auditService.LogAccessAsync(userId, "[EntityName]", id);

                var dto = _mapper.Map<[DtoName]>(entity);
                return Ok(dto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en Get: {ex}");
                return StatusCode(500, new { message = "Ocurrió un error inesperado" });
            }
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
```

### Plantilla 5: Unit Test

```csharp
using Xunit;
using Moq;
using AutoCheckAML.Api.Business;
using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Data.UnitOfWork;

namespace AutoCheckAML.Tests.Unit.Business
{
    public class [ServiceName]Tests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly [ServiceName] _service;

        public [ServiceName]Tests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _service = new [ServiceName](_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task [MethodName]_WithValidInput_ReturnsSuccess()
        {
            // Arrange
            var input = new [InputType] { [Properties] };
            var expected = new [ReturnType] { [Properties] };
            
            _mockUnitOfWork
                .Setup(u => u.[Repository].GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.[MethodName](input);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Id, result.Id);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task [MethodName]_WithInvalidInput_ThrowsException()
        {
            // Arrange
            var invalidInput = new [InputType] { [InvalidProperties] };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.[MethodName](invalidInput));
        }
    }
}
```

### Plantilla 6: Integration Test

```csharp
using Xunit;
using Microsoft.AspNetCore.TestHost;
using System.Net;
using System.Text.Json;

namespace AutoCheckAML.Tests.Integration
{
    public class [ControllerName]IntegrationTests : IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _factory;
        private HttpClient _httpClient;
        private string _bearerToken;

        public [ControllerName]IntegrationTests()
        {
            _factory = new WebApplicationFactory<Program>();
            _httpClient = _factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            // Login y obtener token
            var loginResponse = await _httpClient.PostAsJsonAsync(
                "/api/v2/auth/login",
                new { username = "admin", password = "admin123" });

            var content = await loginResponse.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            _bearerToken = jsonDoc.RootElement.GetProperty("token").GetString();

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken);
        }

        public async Task DisposeAsync()
        {
            _httpClient?.Dispose();
            _factory?.Dispose();
            await Task.CompletedTask;
        }

        [Fact]
        public async Task GetEntity_WithValidId_Returns200()
        {
            // Act
            var response = await _httpClient.GetAsync("/api/v2/[entities]/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetEntity_Unauthorized_Returns401()
        {
            // Arrange - Clear token
            _httpClient.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _httpClient.GetAsync("/api/v2/[entities]/1");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
```

---

## PLAN DE IMPLEMENTACIÓN POR SPRINT

### SPRINT 1 (Días 1-7): Diseño
**Objetivo:** Diagrama y especificaciones completas

| Día | Tarea | Entregable | Dueño |
|-----|-------|-----------|-------|
| 1 | MER + UML diagrams | 2 archivos mermaid | Arquitecto |
| 2 | Flujos y transiciones | 5 diagramas mermaid | Arquitecto |
| 3 | Casos de uso | 10 casos Cockburn | BA |
| 4 | User stories + AC | 20 historias Gherkin | BA |
| 5 | Permisos/Roles | Catálogo permissions | Arquitecto |
| 6 | JWT + Seguridad | Especificación JWT | Security |
| 7 | Review + Ajustes | Documento final | Team |

---

### SPRINT 2 (Días 8-14): Arquitectura Base
**Objetivo:** Base classes y patrones implementados

| Día | Tarea | Entregable | Dueño |
|-----|-------|-----------|-------|
| 1-2 | BaseEntity + AuditableEntity | Code + Tests | Dev Lead |
| 3 | Specification Pattern | BaseSpecification + 5 ejemplos | Senior Dev |
| 4 | Extended Unit of Work | Interfaz + impl | Dev |
| 5 | Authorization Service | Interfaz + tests | Dev |
| 6 | Audit Service | Interfaz + tests | Dev |
| 7 | Code Review | PRs aprobados | Arquitecto |

---

### SPRINT 3 (Días 15-21): Entidades
**Objetivo:** 16 entidades implementadas + migrations

| Día | Tarea | Entregables |
|-----|-------|-------------|
| 1 | User, Role, Permission | 3 clases + tests |
| 2 | UserRole, RolePermissionMapping, RefreshToken | 3 clases + tests |
| 3 | FormTemplate, FormField, FormFieldValidation | 3 clases + tests |
| 4 | FormSubmission (update), FormSubmissionHistory | 2 clases + tests |
| 5 | AuditLog, AppSettings, UserPreferences | 3 clases + tests |
| 6 | DbContext + Migrations | Migration file + seed |
| 7 | Integration testing | Test verde |

---

### SPRINT 4 (Días 22-28): Servicios
**Objetivo:** 8+ servicios con >90% coverage

| Día | Tarea | Tests |
|-----|-------|-------|
| 1-2 | AuthorizationService | 12 test cases |
| 3 | AuditService | 10 test cases |
| 4 | TokenService mejorado | 8 test cases |
| 5 | PermissionService | 8 test cases |
| 6 | SpecificationEvaluator | 6 test cases |
| 7 | Middleware updates | 4 test cases |

---

### SPRINT 5 (Días 29-35): API v2.0
**Objetivo:** 25+ endpoints con OpenAPI

| Día | Tarea | Endpoints |
|-----|-------|-----------|
| 1-2 | Auth Controller v2 | 5 endpoints |
| 3-4 | Forms Controller v2 | 8 endpoints |
| 5 | Roles & Permissions Controller | 8 endpoints |
| 6 | FormTemplates + Audit Controllers | 8 endpoints |
| 7 | Swagger + Documentation | OpenAPI ready |

---

### SPRINT 6 (Días 36-42): Testing Completo
**Objetivo:** >80% coverage + security tests

| Día | Tarea | Test Cases |
|-----|-------|-----------|
| 1-2 | Unit tests completos | 50+ cases |
| 3-4 | Integration tests | 25+ cases |
| 5 | Security tests | 10+ cases |
| 6 | Performance tests | Load testing |
| 7 | CI/CD setup | GitHub Actions |

---

### SPRINT 7 (Días 43-49): Documentación
**Objetivo:** Documentación completa + deployment ready

| Día | Tarea | Documentos |
|-----|-------|-----------|
| 1 | README + SETUP_LOCAL | 2 markdown |
| 2 | ARCHITECTURE + DEVELOPMENT_GUIDE | 2 markdown |
| 3 | DEPLOYMENT + TROUBLESHOOTING | 2 markdown |
| 4 | API_REFERENCE + CHANGELOG | 2 markdown |
| 5 | ADR_LOG + Migration Guide | 2 markdown |
| 6 | Wiki + Video tutorials | 5+ videos |
| 7 | Final review | All green ✅ |

---

## PRÓXIMOS PASOS RECOMENDADOS

### Hoy
1. ✅ Revisar este documento en detalle
2. ✅ Crear issue/tickets en backlog
3. ✅ Asignar equipo a sprints

### Esta Semana
4. ⏳ Completar FASE 1 (diagramas)
5. ⏳ Crear repositorio para código
6. ⏳ Setup inicial de CI/CD

### Este Mes
7. ⏳ FASE 2-3 (Arquitectura + Entidades)
8. ⏳ Setup de BD en local
9. ⏳ Testing inicial

---

**Documentacion actualizada:** Junio 2, 2026  
**Estado:** Listo para Ejecución  
**Versión:** 1.0 Final
