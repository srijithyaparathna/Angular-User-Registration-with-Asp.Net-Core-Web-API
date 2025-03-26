using AuthECAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthECAPI.Extensions
{
    // This static class is used to organize extension methods for app configuration.
    public static class AppConfigExtensions
    {
        // This method configures CORS (Cross-Origin Resource Sharing) for the application.
        // CORS allows your API to be accessed from different origins (e.g., frontend app running on a different port).
        public static WebApplication ConfigureCORS(
            this WebApplication app,
            IConfiguration config) // 'config' is used to access app settings, but it's unused in this method.
        {
            // app.UseCors applies the CORS policy to incoming requests.
            app.UseCors(options =>
                options.WithOrigins("http://localhost:4200") // Allows requests only from this origin (Angular app in this case).
                       .AllowAnyMethod() // Allows any HTTP method (GET, POST, PUT, DELETE, etc.).
                       .AllowAnyHeader() // Allows any headers in the requests.
            );

            return app; // Allows method chaining.
        }

        // This method adds app-specific configuration settings to the DI (Dependency Injection) container.
        public static IServiceCollection AddAppConfig(
            this IServiceCollection services,
            IConfiguration config) // 'config' gives access to the app's configuration settings (e.g., appsettings.json).
        {
            // Binds the "AppSettings" section from appsettings.json to the AppSettings class.
            services.Configure<AppSettings>(config.GetSection("AppSettings"));

            return services; // Allows method chaining.
        }
    }
}
