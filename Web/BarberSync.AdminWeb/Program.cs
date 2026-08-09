var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("BarberSyncApi", c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5080"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "admin_default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

app.Run();
