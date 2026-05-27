var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("BarberSyncApi", c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:8080"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin_default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

app.Run();
