using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Application.Services.Catalog;
using BlinkBlink.Application.Services.Identity;
using BlinkBlink.Application.Services.Inventory;
using BlinkBlink.Application.Services.Orders;
using BlinkBlink.Application.Services.Warranty;
using BlinkBlink.Application.Services.Reporting;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;
using BlinkBlink.Infrastructure.Logging;
using BlinkBlink.Infrastructure.Repositories;
using BlinkBlink.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Khởi tạo bộ ghi nhật ký thích ứng cho kênh Web theo môi trường Development/Production
var webLogDirectory = Path.Combine(builder.Environment.ContentRootPath, "logs");
var appLogger = new AdaptiveFileLogger(
    new AppLogContext
    {
        Channel = "Web",
        EnvironmentName = builder.Environment.EnvironmentName,
        DataMode = "SqlServer"
    },
    preferredDirectory: webLogDirectory,
    filePrefix: "web",
    writeToConsole: builder.Environment.IsDevelopment());
builder.Services.AddSingleton<IAppLogger>(appLogger);

// Cấu hình chuỗi kết nối và tiện ích truy xuất CSDL ADO.NET thuần (Tự động dò tìm SQL Server cục bộ)
var candidateConnections = new[]
{
    builder.Configuration.GetConnectionString("DefaultConnection"),
    "Server=.;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;",
    "Server=localhost;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;",
    "Server=.\\MSSQL;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;",
    "Server=(localdb)\\mssqllocaldb;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;"
}.Where(s => !string.IsNullOrWhiteSpace(s));

DatabaseHelper? dbHelper = null;
foreach (var connStr in candidateConnections)
{
    try
    {
        var helper = new DatabaseHelper(connStr!, appLogger);
        if (helper.TestConnection())
        {
            dbHelper = helper;
            var maskedConn = AdaptiveFileLogger.MaskConnectionString(connStr);
            appLogger.LogInfo("WebStartup", $"Đã kết nối thành công cơ sở dữ liệu SQL Server: {maskedConn}");
            Console.WriteLine($"[BlinkBlink.Web] Da ket noi thanh cong co so du lieu SQL Server: {maskedConn}");
            break;
        }
    }
    catch (Exception ex)
    {
        appLogger.LogWarning("WebStartup", $"Thử kết nối SQL Server thất bại ({AdaptiveFileLogger.MaskConnectionString(connStr)}).", ErrorCodes.DatabaseUnavailable, ex);
    }
}

// Nếu chưa có CSDL, kích hoạt cơ chế tự động tạo CSDL và nạp dữ liệu mẫu
if (dbHelper == null)
{
    foreach (var connStr in candidateConnections)
    {
        try
        {
            if (DatabaseHelper.TryProvisionDatabase(connStr!, out var provisionedHelper, appLogger) && provisionedHelper != null)
            {
                dbHelper = provisionedHelper;
                var maskedConn = AdaptiveFileLogger.MaskConnectionString(connStr);
                appLogger.LogInfo("WebStartup", $"Đã tự động khởi tạo thành công cơ sở dữ liệu SQL Server: {maskedConn}");
                Console.WriteLine($"[BlinkBlink.Web] Da tu dong khoi tao thanh cong co so du lieu SQL Server: {maskedConn}");
                break;
            }
        }
        catch (Exception ex)
        {
            appLogger.LogWarning("WebStartup", $"Tự động khởi tạo CSDL thất bại ({AdaptiveFileLogger.MaskConnectionString(connStr)}).", ErrorCodes.DatabaseUnavailable, ex);
        }
    }
}

if (dbHelper == null)
{
    appLogger.DefaultContext.DataMode = "SqlServerUnavailable";
    appLogger.LogError("WebStartup", "Không thể kết nối tới bất kỳ SQL Server instance nào khi khởi động Web.", ErrorCodes.DatabaseUnavailable);
}

dbHelper ??= new DatabaseHelper("Server=(localdb)\\mssqllocaldb;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True", appLogger);
dbHelper.EnsureSchemaCompatibility();
builder.Services.AddSingleton(dbHelper);

// Đăng ký các Kho dữ liệu (Repositories) sử dụng ADO.NET thuần
builder.Services.AddScoped<ISanPhamRepository, SanPhamRepository>();
builder.Services.AddScoped<INhomSanPhamRepository, NhomSanPhamRepository>();
builder.Services.AddScoped<IDanhMucRepository, DanhMucRepository>();
builder.Services.AddScoped<IKhachHangRepository, KhachHangRepository>();
builder.Services.AddScoped<INhanVienRepository, NhanVienRepository>();
builder.Services.AddScoped<INguoiDungRepository, NguoiDungRepository>();
builder.Services.AddScoped<IVaiTroRepository, VaiTroRepository>();
builder.Services.AddScoped<IPhieuNhapKhoRepository, PhieuNhapKhoRepositoryImpl>();
builder.Services.AddScoped<IPhieuKiemKeRepository, PhieuKiemKeRepository>();
builder.Services.AddScoped<IHoaDonRepository, HoaDonRepository>();
builder.Services.AddScoped<IGioHangRepository, GioHangRepository>();
builder.Services.AddScoped<IBaoHanhRepository, BaoHanhRepository>();
builder.Services.AddScoped<IThongKeRepository, ThongKeRepository>();

// Đăng ký các Trường hợp sử dụng (Use Cases)
builder.Services.AddScoped<QuanLySanPhamUseCase>();
builder.Services.AddScoped<QuanLyNhomSanPhamUseCase>();
builder.Services.AddScoped<QuanLyKiemKeUseCase>();
builder.Services.AddScoped<DangNhapUseCase>();
builder.Services.AddScoped<DangKyKhachHangUseCase>();
builder.Services.AddScoped<QuanLyNhanVienUseCase>();
builder.Services.AddScoped<QuanLyKhachHangUseCase>();
builder.Services.AddScoped<XacNhanNhapKhoUseCase>();
builder.Services.AddScoped<DatHangUseCase>();
builder.Services.AddScoped<XuLyDonHangUseCase>();
builder.Services.AddScoped<TiepNhanBaoHanhUseCase>();
builder.Services.AddScoped<XuLyBaoHanhUseCase>();
builder.Services.AddScoped<ThongKeDoanhThuUseCase>();
builder.Services.AddScoped<ThongKeKhoHangUseCase>();

// Cấu hình MVC và RESTful API Controllers
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Blink Blink Jewelry RESTful API", Version = "v1" });
});

// Cấu hình CORS cho phép Web khách hàng (HTML/JS) gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Kích hoạt middleware bắt lỗi và ghi nhật ký thích ứng ở đầu pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

// Cấu hình HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blink Blink Jewelry API v1");
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthorization();

// Ánh xạ Controller cho cả RESTful API [ApiController] và Web MVC Views
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
