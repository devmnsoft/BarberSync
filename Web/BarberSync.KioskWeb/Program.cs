var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".BarberSync.Kiosk.Flow";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});
builder.Services.AddHttpClient("BarberSyncApi", c => c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5080"));
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); }
app.UseStaticFiles(); app.UseRouting(); app.UseSession();
app.MapControllers();
app.MapControllerRoute(name:"default", pattern:"{controller=Home}/{action=Index}/{id?}");
app.Run();
