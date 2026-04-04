using Microsoft.EntityFrameworkCore;
using MinesweeperApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services to the container
builder.Services.AddControllersWithViews();

// Add the database context and tell it to use SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add session services so the app can remember logged in users
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // Set session timeout
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    // Make the session cookie HTTP only
    options.Cookie.HttpOnly = true;

    // Mark session cookie as essential
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Enable session before authorization
app.UseSession();

app.UseAuthorization();

// Set the default route for the MVC app
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
