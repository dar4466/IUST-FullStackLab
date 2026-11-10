using Microsoft.OpenApi.Models;
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
// --- [NEW FOR LAB 8]: Register Named HttpClients for External APIs ---

// 1. HTTP Client for OpenLibrary Book Metadata API
builder.Services.AddHttpClient("OpenLibraryClient", client =>
{
    client.BaseAddress = new Uri("https://openlibrary.org/");
    client.Timeout = TimeSpan.FromSeconds(5); // Fail fast if external API is slow
    client.DefaultRequestHeaders.Add("User-Agent", "MyMvcApp-IUST-FullStackLab/1.0");
});

// 2. HTTP Client for Live USD-to-INR Currency Exchange API
builder.Services.AddHttpClient("CurrencyClient", client =>
{
    client.BaseAddress = new Uri("https://open.er-api.com/");
    client.Timeout = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.Add("User-Agent", "MyMvcApp-IUST-FullStackLab/1.0");
});
// --- [NEW FOR API]: Register Swagger Generator ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MyMvcApp Book Directory API",
        Version = "v1",
        Description = "RESTful API for CRUD data manipulation on the SQLite Book Catalog"
    });

    // Filter Swagger so it only displays REST routes starting with "api/"
    options.DocInclusionPredicate((docName, apiDesc) =>
        apiDesc.RelativePath != null && apiDesc.RelativePath.StartsWith("api/"));
});
// -------------------------------------------------

var app = builder.Build();
// --- [NEW FOR API]: Enable Swagger Middleware ---
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Book Directory API v1");
    options.RoutePrefix = "swagger"; // Accessible at /swagger
});
// ------------------------------------------------


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

// --- [NEW FOR API]: Map Attribute-Routed API Controllers ---
app.MapControllers();
// -----------------------------------------------------------

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Books}/{action=Index}/{id?}");

app.Run();