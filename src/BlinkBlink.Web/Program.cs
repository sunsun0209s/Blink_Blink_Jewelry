using BlinkBlink.Application.Services.Identity;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;
using BlinkBlink.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình kết nối CSDL SQL Server với AppDbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký các Repository của phân hệ Identity vào DI Container
builder.Services.AddScoped<INguoiDungRepository, NguoiDungRepository>();
builder.Services.AddScoped<INhanVienRepository, NhanVienRepository>();
builder.Services.AddScoped<IKhachHangRepository, KhachHangRepository>();
builder.Services.AddScoped<IVaiTroRepository, VaiTroRepository>();

// Đăng ký các Use Case nghiệp vụ của phân hệ Identity
builder.Services.AddScoped<DangNhapUseCase>();
builder.Services.AddScoped<DangKyKhachHangUseCase>();
builder.Services.AddScoped<QuanLyNhanVienUseCase>();

// Cấu hình Xác thực bằng Cookie cho người dùng Web
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/XacThuc/DangNhap";
        options.LogoutPath = "/XacThuc/DangXuat";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

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
app.UseAuthentication();
app.UseAuthorization();

// Tự động kiểm tra và khởi tạo Database cùng các bảng trên SQL Server
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi trong quá trình khởi tạo CSDL SQL Server.");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
