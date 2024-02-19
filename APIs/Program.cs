using APIs;
using APIs.DAL;
using APIs.Dtos;
using APIs.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddEndpointsApiExplorer(); // Essential for Swagger to find your API endpoints
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "My API", Version = "v1" });
});

builder.Services.AddSingleton<IEmailTSender, FakeEmailSender>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
}
app.MapGet("/", () => "Hello World!");

app.MapPost("/login", async (UserLoginDto userDto, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) =>
{
    var result = await signInManager.PasswordSignInAsync(userDto.Email, userDto.Password, isPersistent: false, lockoutOnFailure: false);
    if (result.Succeeded)
    {
        // Handle success (e.g., return JWT token)
    }
    else
    {
        
    }
});

app.MapPost("/register", async (UserRegistrationDto userDto, UserManager<ApplicationUser> userManager) =>
{
    var user = new ApplicationUser { UserName = userDto.Email, Email = userDto.Email };
    var result = await userManager.CreateAsync(user, userDto.Password);
    if (result.Succeeded)
    {
        // Handle success (e.g., return JWT token)
    }
    else
    {
        // Handle failure
    }
});

app.MapPost("/confirm-email", async (string userId, string token, UserManager<ApplicationUser> userManager) =>
{
    var user = await userManager.FindByIdAsync(userId);
    if (user == null) return Results.NotFound("User not found.");

    var result = await userManager.ConfirmEmailAsync(user, token);
    if (result.Succeeded)
    {
        return Results.Ok("Email confirmed successfully.");
    }
    else
    {
        return Results.BadRequest("Failed to confirm email.");
    }
});

app.MapPost("/logout", async (HttpContext httpContext, SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    // Clear the session or token as applicable
    return Results.Ok("Logged out successfully.");
});

app.MapPost("/forgot-password", async (HttpRequest request, string email, UserManager<ApplicationUser> userManager, IEmailTSender emailSender) =>
{
    var user = await userManager.FindByEmailAsync(email);
    if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
    {
        // Don't reveal that the user does not exist or is not confirmed
        return Results.Ok("If your email is registered, you will receive a password reset link.");
    }

    var token = await userManager.GeneratePasswordResetTokenAsync(user);
    // Access HttpContext from the request
    var callbackUrl = $"http://{request.Host}/reset-password?token={Uri.EscapeDataString(token)}&userId={user.Id}";
    await emailSender.SendEmailAsync(email, "Reset Password",
        $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

    return Results.Ok("If your email is registered, you will receive a password reset link.");
});

app.MapPost("/reset-password", async (string token, string userId, string newPassword, UserManager<ApplicationUser> userManager) =>
{
    var user = await userManager.FindByIdAsync(userId);
    if (user == null) return Results.BadRequest("User not found.");

    var result = await userManager.ResetPasswordAsync(user, token, newPassword);
    if (result.Succeeded)
    {
        return Results.Ok("Password has been reset successfully.");
    }
    else
    {
        return Results.BadRequest("Error resetting password.");
    }
});





app.Run();
