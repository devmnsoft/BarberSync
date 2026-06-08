var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("BarberSyncApi", c => c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? builder.Configuration["ApiBaseUrl"] ?? "http://localhost:8080"));
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); }
app.UseStaticFiles(); app.UseRouting();
app.MapControllers();
app.MapControllerRoute(name:"default", pattern:"{controller=Home}/{action=Index}/{id?}");
app.Run();
