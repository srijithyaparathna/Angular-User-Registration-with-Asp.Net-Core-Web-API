// Import necessary namespaces
using AuthECAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================ Configure Services ============================

// Add controllers to the service container, enabling the use of API controllers.
builder.Services.AddControllers();

// Add support for API documentation using Swagger (for testing and documentation purposes).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== Configure Identity (Authentication and Authorization) =====
// Registers the Identity services with the AppUser model and AppDbContext.
// - AppUser: Custom user model that extends IdentityUser.
// - AppDbContext: Custom DbContext that handles database operations.
builder.Services
    .AddIdentityApiEndpoints<AppUser>()  // Adds minimal API endpoints for Identity management (like login, logout, etc.).
    .AddEntityFrameworkStores<AppDbContext>();  // Configures Identity to use Entity Framework for persisting data in AppDbContext.

// ===== Configure Identity Options =====
// These settings customize the default password and user requirements.
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;  // No need for numbers in the password.
    options.Password.RequireUppercase = false;  // No need for uppercase letters.
    options.Password.RequireLowercase = false;  // No need for lowercase letters.
    options.User.RequireUniqueEmail = true;  // Ensures no two users have the same email.
});

// ===== Configure Database Connection =====
// Configures the app to use SQL Server with the connection string from appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevDB")));

// ============================ Configure Middleware ============================

var app = builder.Build();

// ===== Configure the HTTP request pipeline =====
// These middlewares handle requests and responses.

if (app.Environment.IsDevelopment())
{
    // Enable Swagger only in development mode to avoid exposing it in production.
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ===== Configure CORS (Cross-Origin Resource Sharing) =====
// This allows requests from your Angular app running on http://localhost:4200.
app.UseCors(options =>
    options.WithOrigins("http://localhost:4200")  // Allow requests only from this origin.
           .AllowAnyMethod()  // Allow all HTTP methods (GET, POST, PUT, DELETE, etc.).
           .AllowAnyHeader());  // Allow all headers.

// ===== Authorization Middleware =====
// Ensures only authenticated users can access protected endpoints.
app.UseAuthorization();

// ===== Map Controllers =====
// Maps the controller actions to the app's request pipeline.
app.MapControllers();

// ===== Map Identity API Endpoints =====
// Provides predefined endpoints for user authentication (like login, logout, and user management).
app
    .MapGroup("/api")  // Group all identity-related endpoints under the "/api" route.
    .MapIdentityApi<AppUser>();  // Exposes Identity API endpoints (e.g., /api/login, /api/register).

// ============================ Custom Endpoints ============================

// Custom endpoint for user registration.
// This creates a new user when the frontend sends a POST request to /api/signup.
app.MapPost("/api/signup", async (
    UserManager<AppUser> userManager,  // Provides methods to manage users (e.g., create, delete, find, etc.).
    [FromBody] UserRegistrationModel userRegistrationModel  // Binds the request body to this model.
) =>
{
    // Create a new AppUser instance with the provided data.
    AppUser user = new AppUser()
    {
        UserName = userRegistrationModel.Email,  // Use email as the username.
        Email = userRegistrationModel.Email,
        FullName = userRegistrationModel.FullName,
    };

    // Attempt to create the user with the provided password.
    var result = await userManager.CreateAsync(
        user,
        userRegistrationModel.Password);

    // Return appropriate HTTP responses based on the result.
    if (result.Succeeded)
        return Results.Ok(result);  // Returns 200 OK if user creation is successful.
    else
        return Results.BadRequest(result);  // Returns 400 Bad Request if something goes wrong.
});

// Start the application.
app.Run();

// ============================ Data Models ============================
// This model is used to bind incoming registration data from the client.
public class UserRegistrationModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
}
