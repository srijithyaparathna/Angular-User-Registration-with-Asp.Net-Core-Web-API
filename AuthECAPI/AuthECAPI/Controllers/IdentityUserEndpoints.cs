using AuthECAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthECAPI.Controllers
{
    // Model to handle user registration data
    public class UserRegistrationModel
    {
        public string Email { get; set; }  // User's email
        public string Password { get; set; }  // User's password
        public string FullName { get; set; }  // User's full name
    }

    // Model to handle login data
    public class LoginModel
    {
        public string Email { get; set; }  // User's email
        public string Password { get; set; }  // User's password
    }

    // Static class to define identity-related endpoints
    public static class IdentityUserEndpoints
    {
        // Method to map the user-related endpoints
        public static IEndpointRouteBuilder MapIdentityUserEndpoints(this IEndpointRouteBuilder app)
        {
            // Map the POST /signup endpoint to the CreateUser method
            app.MapPost("/signup", CreateUser);
            // Map the POST /signin endpoint to the SignIn method
            app.MapPost("/signin", SignIn);
            return app;
        }

        // Method to handle user registration (signup)
        private static async Task<IResult> CreateUser(
            UserManager<AppUser> userManager,  // UserManager for handling user-related operations
            [FromBody] UserRegistrationModel userRegistrationModel)  // Input from the request body
        {
            // Create a new AppUser object with the provided registration data
            AppUser user = new AppUser()
            {
                UserName = userRegistrationModel.Email,
                Email = userRegistrationModel.Email,
                FullName = userRegistrationModel.FullName,
            };

            // Create the user with the provided password
            var result = await userManager.CreateAsync(user, userRegistrationModel.Password);

            // Return appropriate response based on the result of user creation
            if (result.Succeeded)
                return Results.Ok(result);  // Return 200 OK if successful
            else
                return Results.BadRequest(result);  // Return 400 Bad Request if failed
        }

        // Method to handle user authentication (signin)
        private static async Task<IResult> SignIn(
            UserManager<AppUser> userManager,  // UserManager for handling user-related operations
            [FromBody] LoginModel loginModel,  // Input from the request body
            IOptions<AppSettings> appSettings)  // App settings to get the JWT secret
        {
            // Find the user by email
            var user = await userManager.FindByEmailAsync(loginModel.Email);
            // Check if the user exists and the password is correct
            if (user != null && await userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                // Generate a JWT token if the credentials are correct
                var signInKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(appSettings.Value.JWTSecret)  // Secret key for signing the token
                );

                // Define the token descriptor with claims and expiration
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UserID", user.Id.ToString())  // Include the user ID in the token
                    }),
                    Expires = DateTime.UtcNow.AddDays(10),  // Token expiration time (10 days)
                    SigningCredentials = new SigningCredentials(
                        signInKey,
                        SecurityAlgorithms.HmacSha256Signature  // Use HMAC SHA256 for signing
                    )
                };

                // Create the token and return it in the response
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);

                return Results.Ok(new { token });  // Return the generated token
            }
            else
            {
                // Return error if the credentials are incorrect
                return Results.BadRequest(new { message = "Username or password is incorrect." });
            }
        }
    }
}
