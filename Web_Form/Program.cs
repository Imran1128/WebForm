using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using Web_Form.Data;
using Web_Form.Interfaces;
using Web_Form.Models;
using Web_Form.Repository;
using DbContext = Web_Form.Data.DbContext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => options.LoginPath = "/Account/Login");


builder.Services.AddScoped<IFormService, FormRepository>();
builder.Services.AddSingleton<IEmailSender, EmailSender>(); // Register the custom email sender

builder.Services.AddDbContext<DbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;  // Allow login without confirmation for testing
})
.AddEntityFrameworkStores<DbContext>()
.AddDefaultTokenProviders();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10, // Allow 10 requests
                Window = TimeSpan.FromSeconds(30), // In 30 seconds
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2 // Queue 2 additional requests
            });
    });
});


// Add Razor Pages services
builder.Services.AddRazorPages();

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


app.UseRouting();
app.UseSession();
app.MapRazorPages();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Forms}/{action=Index}/{id?}");

app.Run();
