// Program.cs
using DocumentManagementML.API.Extensions;
using DocumentManagementML.API.Filters;
using DocumentManagementML.API.Middleware;
using DocumentManagementML.API.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using System.Threading.Tasks;
using DocumentManagementML.API.Auth;
using DocumentManagementML.API.Validators;
using DocumentManagementML.Domain.Repositories;
using DocumentManagementML.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on all interfaces
builder.WebHost.ConfigureKestrel(options => {
    // Clear any existing endpoints
    options.ConfigureEndpointDefaults(opt => { });
    
    // Listen on IPv4 and IPv6 addresses
    options.ListenAnyIP(5149);
    
    // Explicitly add localhost bindings
    options.Listen(System.Net.IPAddress.Parse("127.0.0.1"), 5149);
    options.Listen(System.Net.IPAddress.Parse("::1"), 5149);
});

// Add services to the container
builder.Services.AddControllers(options =>
{
    // Add validation filter to automatically validate models
    options.Filters.Add<DocumentManagementML.API.Filters.ValidationFilter>();
    
    // Configure Problem Details
    options.EnableEndpointRouting = false;
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Configure Problem Details
builder.Services.AddProblemDetails(options => 
{
    // Customize problem details
    options.CustomizeProblemDetails = context =>
    {
        // Add application-specific properties
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["serverId"] = Environment.MachineName;
    };
});

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Add API version explorer
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Determine whether to use enhanced controllers from configuration
bool useEnhanced = builder.Configuration.GetValue<bool>("UseEnhancedControllers", true);
builder.Services.UseEnhancedControllers(useEnhanced);

Console.WriteLine($"Using {(useEnhanced ? "enhanced" : "original")} controllers");

// Register validators
builder.Services.AddScoped<IValidator<DocumentManagementML.Application.DTOs.DocumentCreateDto>, DocumentManagementML.API.Validators.DocumentCreateDtoValidator>();
builder.Services.AddScoped<IValidator<DocumentManagementML.Application.DTOs.DocumentUpdateDto>, DocumentManagementML.API.Validators.DocumentUpdateDtoValidator>();
builder.Services.AddScoped<IValidator<DocumentManagementML.Application.DTOs.DocumentTypeCreateDto>, DocumentManagementML.API.Validators.DocumentTypeCreateDtoValidator>();
builder.Services.AddScoped<IValidator<DocumentManagementML.Application.DTOs.DocumentTypeUpdateDto>, DocumentManagementML.API.Validators.DocumentTypeUpdateDtoValidator>();
builder.Services.AddScoped<IValidator<DocumentManagementML.Application.DTOs.UserCreateDto>, UserValidators.UserCreateDtoValidator>();
builder.Services.AddScoped<IValidator<DocumentManagementML.Application.DTOs.UserUpdateDto>, UserValidators.UserUpdateDtoValidator>();
builder.Services.AddScoped<IValidator<DocumentManagementML.API.Auth.LoginRequest>, UserValidators.LoginRequestValidator>();
builder.Services.AddScoped<IValidator<DocumentManagementML.API.Auth.RefreshTokenRequest>, UserValidators.RefreshTokenRequestValidator>();
builder.Services.AddScoped<IValidator<DocumentManagementML.API.Auth.RegisterRequest>, UserValidators.RegisterRequestValidator>();
builder.Services.AddScoped<IValidator<DocumentManagementML.API.Validators.ChangePasswordRequest>, DocumentManagementML.API.Validators.ChangePasswordRequestValidator>();

// Add Swagger documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Set the comments path for the Swagger JSON and UI
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    c.UseInlineDefinitionsForEnums();
    
    // Add a security definition for JWT Bearer authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure versioned Swagger
builder.Services.ConfigureVersionedSwagger();

// Add infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add application services
builder.Services.AddApplicationServices();

// Add ML services
builder.Services.AddMLServices(builder.Configuration);

// Add storage services
builder.Services.AddStorageServices(builder.Configuration);

// Add request throttling
builder.Services.AddRequestThrottling(builder.Configuration);

// Add JWT authentication
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
    var key = Encoding.ASCII.GetBytes(jwtSettings?.SecretKey ?? string.Empty);
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Add("X-Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        // Configure Swagger UI for versioned API
        var provider = app.Services.GetRequiredService<Microsoft.AspNetCore.Mvc.ApiExplorer.IApiVersionDescriptionProvider>();
        
        // Build a swagger endpoint for each discovered API version
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Document Management ML API {description.GroupName.ToUpperInvariant()}");
        }
        
        // For backward compatibility
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Management ML API v1");
    });
}

app.UseHttpsRedirection();
app.UseRouting();

// Add request logging middleware
app.UseMiddleware<DocumentManagementML.API.Middleware.RequestLoggingMiddleware>();

// Add Problem Details middleware (replacing ValidationExceptionMiddleware)
app.UseMiddleware<DocumentManagementML.API.Middleware.ProblemDetailsMiddleware>();

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Add role-based authorization middleware
app.UseRoleBasedAuthorization();

// Add request throttling
app.UseRequestThrottling();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Seed the database if it's empty
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<DocumentManagementML.Infrastructure.Data.DocumentManagementDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<DocumentManagementML.Application.Interfaces.IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<DocumentManagementML.Infrastructure.Services.DatabaseSeeder>>();
        
        // Convert interface to concrete type for seeder
        var concretePasswordHasher = passwordHasher as DocumentManagementML.Infrastructure.Services.SimplePasswordHasher;
        if (concretePasswordHasher == null)
        {
            Console.WriteLine("⚠️  Password hasher not available for seeding, skipping user creation");
            return;
        }
        
        var seeder = new DocumentManagementML.Infrastructure.Services.DatabaseSeeder(context, concretePasswordHasher, logger);
        var seeded = await seeder.SeedAsync();
        
        if (seeded)
        {
            Console.WriteLine("✅ Database seeded successfully with initial data");
        }
        else
        {
            Console.WriteLine("ℹ️  Database already contains data, seeding skipped");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error seeding database: {ex.Message}");
    }
}

app.Run();