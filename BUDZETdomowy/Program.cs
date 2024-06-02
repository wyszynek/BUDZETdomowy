using HomeBudget.Data;
using HomeBudget.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index"; // Path to the login form
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

var connectionString = builder.Configuration.GetConnectionString("System");
builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(connectionString));

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// Register the background service
builder.Services.AddHostedService<RecurringPaymentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors(
    options => options
    .AllowAnyHeader()
    .AllowAnyOrigin()
    .AllowAnyMethod()
);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
