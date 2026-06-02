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
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add DbContext with SQLite
builder.Services.AddDbContext<AutoCheckAMLContext>(options =>
    options.UseSqlite("Data Source=autocheckaml.db"));

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

// Add Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

// Create database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AutoCheckAMLContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Use Custom Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Use CORS
app.UseCors("AllowAll");

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
