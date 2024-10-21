using Microsoft.EntityFrameworkCore;
using WebApp_CRUD.Data;
using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using NToastNotify;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebApp_CRUD.Helper;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình toast notifications với NToastNotify
builder.Services.AddRazorPages().AddNToastNotifyNoty(new NotyOptions
{
    ProgressBar = true,
    Timeout = 5000 // Thời gian thông báo hiển thị
});

// Cấu hình toast notifications với Notyf
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 5; // Thời gian hiển thị
    config.IsDismissable = true;   // Cho phép người dùng đóng thông báo
    config.Position = NotyfPosition.TopRight; // Vị trí hiển thị thông báo
    config.HasRippleEffect = true; // Hiệu ứng ripple
});

// Thêm dịch vụ cho Controllers và Views
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".tfl.session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Authentication - Authorize Servicess
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    //options.AccessDeniedPath = new PathString("/Manager/Home/Index");
    options.Cookie = new CookieBuilder
    {
        HttpOnly = true,
        Name = "tfl.cookie",
        Path = "/",
        SameSite = SameSiteMode.Lax,
        SecurePolicy = CookieSecurePolicy.SameAsRequest
    };
    options.LoginPath = "/Admin/Login";
    options.ReturnUrlParameter = "returnUrl";
    options.SlidingExpiration = true;
});

// Cấu hình DbContext cho Entity Framework
builder.Services.AddDbContext<DataAppMvcContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")); // Kết nối đến cơ sở dữ liệu
});

// Cấu hình AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();

// Cấu hình middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Bảo mật thông qua HSTS
}

// Cấu hình pipeline cho các yêu cầu HTTP
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

// Thêm xác thực (authentication) và phân quyền (authorization)
app.UseCookiePolicy();
app.UseAuthentication(); // Đăng nhập, bảo mật
app.UseAuthorization(); // Phân quyền

// Sử dụng thông báo toast
app.UseNToastNotify();
app.UseNotyf();

// Định tuyến cho các controller Admin
app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
