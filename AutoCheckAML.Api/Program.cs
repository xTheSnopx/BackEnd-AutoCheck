using AutoCheckAML.Api.Data;
using AutoCheckAML.Api.Data.UnitOfWork;
using AutoCheckAML.Api.Business;
using AutoCheckAML.Api.Helpers.Logging;
using AutoCheckAML.Api.Web.Middleware;
using AutoCheckAML.Api.Web.Mapping;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add DbContext with SQLite
builder.Services.AddDbContext<AutoCheckAMLContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=autocheckaml.db"));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add FluentValidation - Registrar validadores manualmente
var assembly = typeof(Program).Assembly;
var validatorType = typeof(IValidator<>);
var validatorTypes = assembly.GetTypes()
    .Where(t => t.GetInterfaces().Any(i =>
        i.IsGenericType &&
        i.GetGenericTypeDefinition() == validatorType))
    .ToList();

foreach (var type in validatorTypes)
{
    var interfaces = type.GetInterfaces()
        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == validatorType);
    
    foreach (var interfaceType in interfaces)
    {
        builder.Services.AddScoped(interfaceType, type);
    }
}

// Add Unit of Work Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add Business Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICrewService, CrewService>();
builder.Services.AddScoped<IRoleService, RoleService>();

// Add Logger Service
builder.Services.AddScoped<ILoggerService, LoggerService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT configuration is missing in appsettings.json");
}

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add OpenAPI
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "AutoCheckAML API";
        document.Info.Version = "v1";
        document.Info.Description = "API para gestión de inspecciones de flota y cumplimiento AML";
        document.Info.Contact = new OpenApiContact
        {
            Name = "Soporte AutoCheckAML",
            Email = "soporte@autocheckaml.com"
        };

        // Add Bearer Token security scheme
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
        });

        var requirements = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document),
                new List<string>()
            }
        };
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(requirements);

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Create database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AutoCheckAMLContext>();
    dbContext.Database.EnsureCreated();
}

// Ensure admin user exists with correct password
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AutoCheckAMLContext>();
    var adminUser = await dbContext.Users
        .FirstOrDefaultAsync(u => (u.Username.ToLower() == "admin" || u.Email == "admin@autocheck.com") && !u.IsDeleted);

    if (adminUser == null)
    {
        adminUser = new AutoCheckAML.Api.Entity.User
        {
            Username = "Admin",
            Email = "admin@autocheck.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin2026"),
            FullName = "Administrador del Sistema",
            IsActive = true,
            CrewId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            DeletedAt = null,
            IsDeleted = false,
            LastLogin = null,
            LastModifiedBy = null,
            DeletedBy = null
        };
        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync();
    }
    else
    {
        bool changed = false;
        if (adminUser.Username != "Admin")
        {
            adminUser.Username = "Admin";
            changed = true;
        }
        if (!BCrypt.Net.BCrypt.Verify("Admin2026", adminUser.PasswordHash))
        {
            adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin2026");
            changed = true;
        }
        if (changed)
        {
            adminUser.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }
    }

    // Ensure DEV role assignment exists
    var adminRole = await dbContext.UserRoles
        .FirstOrDefaultAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == 1);
    if (adminRole == null)
    {
        dbContext.UserRoles.Add(new AutoCheckAML.Api.Entity.UserRole
        {
            UserId = adminUser.Id,
            RoleId = 1,
            AssignedAt = DateTime.UtcNow,
            ExpiresAt = null,
            IsActive = true
        });
        await dbContext.SaveChangesAsync();
    }

    // Helper local para asegurar la existencia de usuarios de prueba
    async Task EnsureTestUser(string username, string email, string password, string fullName, int roleId)
    {
        var testUser = await dbContext.Users
            .FirstOrDefaultAsync(u => (u.Username.ToLower() == username.ToLower() || u.Email == email) && !u.IsDeleted);

        if (testUser == null)
        {
            testUser = new AutoCheckAML.Api.Entity.User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName = fullName,
                IsActive = true,
                CrewId = null,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            dbContext.Users.Add(testUser);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            bool changed = false;
            if (!BCrypt.Net.BCrypt.Verify(password, testUser.PasswordHash))
            {
                testUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                changed = true;
            }
            if (changed)
            {
                testUser.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }
        }

        var testUserRole = await dbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == testUser.Id && ur.RoleId == roleId);
        if (testUserRole == null)
        {
            dbContext.UserRoles.Add(new AutoCheckAML.Api.Entity.UserRole
            {
                UserId = testUser.Id,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            });
            await dbContext.SaveChangesAsync();
        }
    }

    await EnsureTestUser("ingeniero", "ingeniero@autocheck.com", "Ingeniero2026", "Ingeniero Mecánico Juan", 3);
    await EnsureTestUser("hsq", "hsq@autocheck.com", "Hsq2026", "Ingeniero HSQ Maria", 4);
    await EnsureTestUser("cuadrilla", "cuadrilla@autocheck.com", "Cuadrilla2026", "Operador Cuadrilla Carlos", 5);

    // Ensure default FormTemplate and FormFields exist
    var defaultTemplate = await dbContext.FormTemplates.FirstOrDefaultAsync(t => t.Id == 1);
    if (defaultTemplate == null)
    {
        defaultTemplate = new AutoCheckAML.Api.Entity.FormTemplate
        {
            Id = 1,
            Name = "Inspección de Vehículo AutoCheck",
            Description = "Formulario de inspección técnica diaria del vehículo y cumplimiento del operador",
            FormType = "Mecánico",
            Version = 1,
            IsActive = true,
            RequiresSignature = true,
            DisplayOrder = 1,
            CreatedByUserId = adminUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        
        dbContext.FormTemplates.Add(defaultTemplate);
        await dbContext.SaveChangesAsync();

        var fields = new List<AutoCheckAML.Api.Entity.FormField>
        {
            // Category 1: Documentos
            new() { Id = 1, FormTemplateId = 1, Label = "Permiso de Circulación al Día", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 1, Options = "[\"SI\", \"NO\", \"NA\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, FormTemplateId = 1, Label = "Revisión Tecnomecánica al Día", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 2, Options = "[\"SI\", \"NO\", \"NA\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 3, FormTemplateId = 1, Label = "SOAT Vigente", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 3, Options = "[\"SI\", \"NO\", \"NA\"]", IsActive = true, CreatedAt = DateTime.UtcNow },

            // Category 2: Operador
            new() { Id = 4, FormTemplateId = 1, Label = "Licencia Municipal", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 4, Options = "[\"SI\", \"NO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 5, FormTemplateId = 1, Label = "Curso de Operador", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 5, Options = "[\"SI\", \"NO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 6, FormTemplateId = 1, Label = "Licencia Interna", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 6, Options = "[\"SI\", \"NO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },

            // Category 3: Sistemas - Luces
            new() { Id = 7, FormTemplateId = 1, Label = "Altas / Bajas", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 7, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 8, FormTemplateId = 1, Label = "Retroceso / Emergencia", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 8, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 9, FormTemplateId = 1, Label = "Laterales (Delante/Trasera)", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 9, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 10, FormTemplateId = 1, Label = "Freno / Estacionamiento", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 10, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 11, FormTemplateId = 1, Label = "Cabina Interior", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 11, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },

            // Category 3: Sistemas - Neumáticos
            new() { Id = 12, FormTemplateId = 1, Label = "DEL. IZQUIERDO", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 12, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 13, FormTemplateId = 1, Label = "DEL. DERECHO", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 13, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 14, FormTemplateId = 1, Label = "TRAS. INTERNOS", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 14, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 15, FormTemplateId = 1, Label = "TRAS. EXTERNOS", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 15, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 16, FormTemplateId = 1, Label = "REPUESTOS", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 16, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },

            // Category 3: Sistemas - Accesorios
            new() { Id = 17, FormTemplateId = 1, Label = "Extintor/Botiquín", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 17, Options = "[\"SI\", \"NO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 18, FormTemplateId = 1, Label = "Conos/Bocina", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 18, Options = "[\"SI\", \"NO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 19, FormTemplateId = 1, Label = "Sirena/Licuadora", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 19, Options = "[\"SI\", \"NO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 20, FormTemplateId = 1, Label = "Evaluador Cop.", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 20, Options = "[\"SI\", \"NO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 21, FormTemplateId = 1, Label = "Cortacorrientes / Sist. Comunicación", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 21, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },

            // Category 3: Sistemas - Vidrios
            new() { Id = 22, FormTemplateId = 1, Label = "PARABRISAS", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 22, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 23, FormTemplateId = 1, Label = "ESQUIERDO/DER.", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 23, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },

            // Category 3: Sistemas - Espejos
            new() { Id = 24, FormTemplateId = 1, Label = "LATERALES", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 24, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 25, FormTemplateId = 1, Label = "CABINA", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 25, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },

            // Category 3: Sistemas - Estabilizadores
            new() { Id = 26, FormTemplateId = 1, Label = "Delant. Izq/Der", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 26, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 27, FormTemplateId = 1, Label = "Tras. Izq/Der", Description = "", ValidationRules = "{}", DefaultValue = "", FieldType = "Select", IsRequired = true, DisplayOrder = 27, Options = "[\"OK\", \"FALLO\"]", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        dbContext.FormFields.AddRange(fields);
        await dbContext.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline
app.MapOpenApi();
app.MapScalarApiReference("/scalar", options =>
{
    options.WithTitle("AutoCheckAML API")
           .WithTheme(ScalarTheme.DeepSpace)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});
app.MapGet("/swagger", () => Results.Redirect("/scalar"));

// Use Custom Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Use CORS
app.UseCors("AllowAll");

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:5000");
