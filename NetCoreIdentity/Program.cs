using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetCoreIdentity.EmailServices;
using NetCoreIdentity.Context.Models;
using NetCoreIdentity.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.Configuration;
using Microsoft.DotNet.Scaffolding.Shared;


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
var serviceProvider = builder.Services.BuildServiceProvider();
var configuration = serviceProvider.GetRequiredService<IConfiguration>();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); //PATH

builder.Services.AddIdentity<User,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>
(i =>

    new SmtpEmailSender(

      builder.Configuration["EmailSender:Host"],

      builder.Configuration.GetValue<int>("EmailSender:Port"),

      builder.Configuration.GetValue<bool>("EmailSender:EnableSSL"),

      builder.Configuration["EmailSender:UserName"],

      builder.Configuration["EmailSender:Password"]
));

//Identity Ayarlarý
builder.Services.Configure<IdentityOptions>(options =>
{
    //password ayarlarý
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;

    //lockout ayarlarý
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;

});

//cookie ayarlarý
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/accessdenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.Cookie = new CookieBuilder
    {
        HttpOnly = true,
        Name = ".Identity.Security.Cookie",
        SameSite = SameSiteMode.Strict
    };
});

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

app.UseAuthorization();


var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

using (var scope = scopeFactory.CreateScope())

{

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    SeedIdentity.Seed(userManager, roleManager, configuration).Wait();

}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
