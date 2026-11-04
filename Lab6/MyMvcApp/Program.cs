using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Existing Database Context Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Register ASP.NET Core Identity Services
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- DATABASE SEEDER START ---
// This must run AFTER builder.Build() because it requires the 'app' variable
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // 1. Create the "Admin" role if it doesn't exist in the database
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // 2. Create a default Admin User if they don't exist
    string adminEmail = "admin@mylibrary.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        // Create the user profile
        adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        
        // Save the user with a strong password (Identity requires Upper, Lower, Number, and Special Char by default)
        await userManager.CreateAsync(adminUser, "Admin@123!"); 
        
        // Assign the new user to the Admin role
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
// --- DATABASE SEEDER END ---

app.UseStaticFiles();
app.UseRouting();

// 3. Security Middleware (Order matters)
app.UseAuthentication(); // Verifies WHO the user is
app.UseAuthorization();  // Verifies WHAT the user can access

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Books}/{action=Index}/{id?}");

// 4. Map Identity Razor Pages (Required for the built-in Login/Register UI)
app.MapRazorPages(); 
// Automatically redirect the empty root URL to your new Books routing
app.MapGet("/", () => Results.Redirect("/Library/Books"));

app.Run();