/*
 *
 *

Scaffold-DbContext "Data Source=112.78.2.42,1433;Initial Catalog=gol82750_Ecoffee1;Persist Security Info=True;User ID=gol82750_Ecoffee1;password=Ecoffee121@;Trust Server Certificate=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Force

dotnet ef dbcontext scaffold  "Data Source=112.78.2.42,1433;Initial Catalog=gol82750_Ecoffee1;Persist Security Info=True;User ID=gol82750_Ecoffee1;Password=Ecoffee121@;Trust Server Certificate=True" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --force
 * 
 */
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Globalization;
using CoffeeManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;


var builder = WebApplication.CreateBuilder(args);


// Database server connection
builder.Services.AddDbContext<Gol82750Ecoffee1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CoffeeManagement")));


//// Add Runtime Compilation
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Add Authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/";
    });

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// Config Lowercase
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

/*
 *  BUILD APP
 */
var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();
app.UseSession();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();

