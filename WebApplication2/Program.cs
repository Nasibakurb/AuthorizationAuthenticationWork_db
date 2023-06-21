using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// ƒобавление сервисов в контейнер зависимостей
builder.Services.AddControllersWithViews();

// добавл. схемы аутент. в сервисы приложение (аутент. на основе cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "CookieUsername"; // задайте им€ cookie
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10); // врем€ жизни cookie после пропадает аутент.
        options.LoginPath = "/Home/Login"; // установите путь к странице входа если польз. неаутент.
    });

// добавл. настройки и определ. политику авторизации
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder() // устанавл. политику авториз. пол умолчанию
        .RequireAuthenticatedUser() // “ребовать аутентифицированного пользовател€
        .Build(); // заверш. настройки политики авторизации
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

// включ. обработ. аутент и авторизац. в процессе обработки запросов.
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
