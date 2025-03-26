// Import necessary namespaces for authentication, identity, entity framework, JWT, etc.
using AuthECAPI.Controllers;
using AuthECAPI.Extensions;
using AuthECAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Create a WebApplication builder to set up and configure the app.
var builder = WebApplication.CreateBuilder(args);

// =============================
// Add services to the container.
// =============================

// Add controller services to handle HTTP requests.
builder.Services.AddControllers();

// Add custom service configurations using extension methods for cleaner code.
builder.Services
    .AddSwaggerExplorer()              // Adds Swagger (API documentation) configuration.
    .InjectDbContext(builder.Configuration) // Injects the database context using the app's configuration.
    .AddAppConfig(builder.Configuration)    // Adds additional app-specific configurations.
    .AddIdentityHandlersAndStores()   // Configures Identity for user management.
    .ConfigureIdentityOptions()       // Sets Identity options like password policies, etc.
    .AddIdentityAuth(builder.Configuration); // Adds authentication mechanisms, especially JWT handling.

// Build the app with the configured services.
var app = builder.Build();

// ======================================
// Configure the HTTP request pipeline.
// ======================================

// Enable Swagger for API documentation and testing.
app.ConfigureSwaggerExplorer();

// Set up Cross-Origin Resource Sharing (CORS) to control access from different origins.
app.ConfigureCORS(builder.Configuration);

// Add authentication and authorization middlewares to handle JWT and user identity.
app.AddIdentityAuthMiddlewares();

// Map controller routes to handle incoming requests through the controllers.
app.MapControllers();

// Map Identity API endpoints under the "/api" route to handle user authentication and registration.
app.MapGroup("/api")
   .MapIdentityApi<AppUser>();

// Map additional user-related endpoints under the "/api" route for user-specific operations.
app.MapGroup("/api")
   .MapIdentityUserEndpoints();

// Start running the application and listen for incoming HTTP requests.
app.Run();
