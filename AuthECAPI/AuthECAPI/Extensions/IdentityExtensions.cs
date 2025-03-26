using AuthECAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthECAPI.Extensions
{
    public static class IdentityExtensions
    {
        // This method adds Identity services to the DI (Dependency Injection) container
        // and configures the app to use Identity API endpoints along with Entity Framework stores.
        public static IServiceCollection AddIdentityHandlersAndStores(this IServiceCollection services)
        {
            services.AddIdentityApiEndpoints<AppUser>()  // Adds API endpoints for Identity (like login, register, etc.)
                    .AddEntityFrameworkStores<AppDbContext>(); // Configures Identity to use EF Core for storing user info
            return services;
        }

        // This method configures Identity options like password complexity and unique email requirements.
        public static IServiceCollection ConfigureIdentityOptions(this IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings - Customize as needed
                options.Password.RequireDigit = false; // No need for digits in passwords
                options.Password.RequireUppercase = false; // No need for uppercase letters in passwords
                options.Password.RequireLowercase = false; // No need for lowercase letters in passwords

                // User settings
                options.User.RequireUniqueEmail = true; // Each user must have a unique email address
            });
            return services;
        }

        // This method sets up Authentication and Authorization using JWT tokens.
        public static IServiceCollection AddIdentityAuth(
            this IServiceCollection services,
            IConfiguration config)
        {
            // Configures authentication schemes to use JWT Bearer tokens
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme =
                x.DefaultChallengeScheme =
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(y =>
            {
                y.SaveToken = false; // Don't store the token on the server side; use it directly in each request

                // Configures JWT token validation parameters
                y.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // Ensure the token is signed properly
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            config["AppSettings:JWTSecret"]!)) // Retrieve the secret key from appsettings.json
                };
            });
            return services;
        }

        // This method adds the Authentication and Authorization middlewares to the app pipeline.
        public static WebApplication AddIdentityAuthMiddlewares(this WebApplication app)
        {
            app.UseAuthentication(); // Checks for authentication in each request
            app.UseAuthorization();  // Checks for authorization in each request
            return app;
        }
    }
}
