using JaxSun.Web.Services;
using JaxSun.Web.Data;
using JaxSun.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IIdeaSubmissionService, IdeaSubmissionService>();
builder.Services.AddScoped<ITimeTrackingService, TimeTrackingService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

var app = builder.Build();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Ensure database is created
    await context.Database.EnsureCreatedAsync();

    // Seed roles
    await SeedRolesAsync(roleManager);

    // Seed default user
    await SeedDefaultUserAsync(userManager);

    // Seed default categories
    await SeedTimeCategoriesAsync(context);

    // Migrate existing JSON data if it exists
    await MigrateJsonDataAsync(context, scope.ServiceProvider);
    
    // Check database status
    await CheckDatabaseStatus(app.Services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

// Database seeding methods
static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roles = { "Admin", "Partner" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

static async Task SeedDefaultUserAsync(UserManager<IdentityUser> userManager)
{
    string email = "bjackson@jaxsun.us";
    string password = "Password123";

    Console.WriteLine($"Checking for user: {email}");
    
    if (await userManager.FindByEmailAsync(email) == null)
    {
        Console.WriteLine("User not found, creating...");
        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            Console.WriteLine("User created successfully, adding to Admin role");
            await userManager.AddToRoleAsync(user, "Admin");
        }
        else
        {
            Console.WriteLine($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        Console.WriteLine("User already exists");
    }
}

static async Task SeedTimeCategoriesAsync(ApplicationDbContext context)
{
    if (!context.TimeCategories.Any())
    {
        var categories = new List<TimeCategory>
        {
            new TimeCategory { Name = "Development", Description = "Software development and coding", Color = "#007bff" },
            new TimeCategory { Name = "Design", Description = "UI/UX design and graphics", Color = "#28a745" },
            new TimeCategory { Name = "Business", Description = "Business planning and strategy", Color = "#ffc107" },
            new TimeCategory { Name = "Research", Description = "Market research and analysis", Color = "#17a2b8" },
            new TimeCategory { Name = "Marketing", Description = "Marketing and promotional activities", Color = "#dc3545" },
            new TimeCategory { Name = "Meeting", Description = "Team meetings and client calls", Color = "#6c757d" },
            new TimeCategory { Name = "Testing", Description = "Quality assurance and testing", Color = "#fd7e14" },
            new TimeCategory { Name = "Documentation", Description = "Writing documentation and reports", Color = "#6f42c1" }
        };

        context.TimeCategories.AddRange(categories);
        await context.SaveChangesAsync();
    }
}

static async Task MigrateJsonDataAsync(ApplicationDbContext context, IServiceProvider serviceProvider)
{
    try
    {
        var ideaService = serviceProvider.GetRequiredService<IIdeaSubmissionService>();
        var existingSubmissions = await ideaService.GetAllSubmissionsAsync();

        if (existingSubmissions.Any() && !context.IdeaSubmissions.Any())
        {
            foreach (var submission in existingSubmissions)
            {
                // Reset Id to let EF Core assign new ones
                submission.Id = 0;
                context.IdeaSubmissions.Add(submission);
            }

            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        // Log error but don't fail startup
        Console.WriteLine($"Warning: Could not migrate JSON data: {ex.Message}");
    }
}

static async Task CheckDatabaseStatus(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    Console.WriteLine("=== Database Check ===");
    
    try
    {
        // Check if database exists
        var canConnect = await context.Database.CanConnectAsync();
        Console.WriteLine($"Database connection: {(canConnect ? "SUCCESS" : "FAILED")}");
        
        if (!canConnect)
        {
            Console.WriteLine("Cannot connect to database.");
            return;
        }
        
        // Check roles
        var roles = await roleManager.Roles.ToListAsync();
        Console.WriteLine($"Roles found: {roles.Count}");
        foreach (var role in roles)
        {
            Console.WriteLine($"  - {role.Name}");
        }
        
        // Check users
        var users = await userManager.Users.ToListAsync();
        Console.WriteLine($"Users found: {users.Count}");
        foreach (var user in users)
        {
            Console.WriteLine($"  - {user.Email} (ID: {user.Id})");
            var userRoles = await userManager.GetRolesAsync(user);
            Console.WriteLine($"    Roles: {string.Join(", ", userRoles)}");
        }
        
        // Check categories
        var categories = await context.TimeCategories.ToListAsync();
        Console.WriteLine($"Time categories: {categories.Count}");
        
        // Check if bjackson user exists and verify password
        var bjackson = await userManager.FindByEmailAsync("bjackson@jaxsun.us");
        if (bjackson != null)
        {
            Console.WriteLine("✅ bjackson@jaxsun.us user found!");
            var passwordCheck = await userManager.CheckPasswordAsync(bjackson, "Password123");
            Console.WriteLine($"Password 'Password123' is valid: {(passwordCheck ? "✅ YES" : "❌ NO")}");
            
            var passwordCheck2 = await userManager.CheckPasswordAsync(bjackson, "Jackson!1");
            Console.WriteLine($"Password 'Jackson!1' is valid: {(passwordCheck2 ? "✅ YES" : "❌ NO")}");
        }
        else
        {
            Console.WriteLine("❌ bjackson@jaxsun.us user NOT found!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error checking database: {ex.Message}");
    }
    
    Console.WriteLine("=== End Database Check ===");
}

// Make Program class accessible for testing
public partial class Program { }