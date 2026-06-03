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
        .FirstOrDefaultAsync(u => u.Username == "Admin" && !u.IsDeleted);

    if (adminUser == null)
    {
        adminUser = new AutoCheckAML.Api.Entity.User
        {
            Username = "Admin",
            Email = "admin@autocheck.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin"),
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
        // Update password if it doesn't match
        if (!BCrypt.Net.BCrypt.Verify("Admin", adminUser.PasswordHash))
        {
            adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin");
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
