using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// ���������� �������� � ��������� ������������
builder.Services.AddControllersWithViews();

// ������. ����� ������. � ������� ���������� (������. �� ������ cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "CookieUsername"; // ������� ��� cookie
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10); // ����� ����� cookie ����� ��������� ������.
        options.LoginPath = "/Home/Login"; // ���������� ���� � �������� ����� ���� �����. ��������.
    });

// ������. ��������� � �������. �������� �����������
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder() // ��������. �������� �������. ��� ���������
        .RequireAuthenticatedUser() // ��������� �������������������� ������������
        .Build(); // ������. ��������� �������� �����������
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

// �����. �������. ������ � ���������. � �������� ��������� ��������.
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
