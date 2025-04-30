// Program.cs
using DocumentManagementML.API.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

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
builder.Services.AddControllers();

// Add Swagger documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Document Management ML API", 
        Version = "v1",
        Description = "API for document management with ML classification"
    });
    
    // Set the comments path for the Swagger JSON and UI
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    c.UseInlineDefinitionsForEnums();
});

// Add infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add application services
builder.Services.AddApplicationServices();

// Add ML services
builder.Services.AddMLServices(builder.Configuration);

// Add storage services
builder.Services.AddStorageServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Management ML API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();