using Microsoft.AspNetCore.Authentication.Cookies;
using BarberSync.AdminWeb.Services.Navigation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IModuleNavigationService,ModuleNavigationService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "BarberSync.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = false;
    });
builder.Services.AddAuthorization(options=>options.AddPolicy("PlatformAccess",policy=>policy.RequireClaim("platform_admin","true")));
builder.Services.AddHttpClient("BarberSyncApi", c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5080"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "admin_default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

app.Run();
