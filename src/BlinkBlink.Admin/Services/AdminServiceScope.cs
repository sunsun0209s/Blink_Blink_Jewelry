using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Application.Common.Logging;
using BlinkBlink.Application.Services.Catalog;
using BlinkBlink.Application.Services.Identity;
using BlinkBlink.Application.Services.Inventory;
using BlinkBlink.Application.Services.Orders;
using BlinkBlink.Application.Services.Reporting;
using BlinkBlink.Application.Services.Warranty;
using BlinkBlink.Domain.Entities.Cart;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Entities.Inventory;
using BlinkBlink.Domain.Entities.Orders;
using BlinkBlink.Domain.Entities.Warranty;
using BlinkBlink.Domain.Exceptions;
using BlinkBlink.Domain.Interfaces;
using BlinkBlink.Infrastructure.Data;
using BlinkBlink.Infrastructure.Logging;
using BlinkBlink.Infrastructure.Repositories;

namespace BlinkBlink.Admin.Services;

/// <summary>
/// Quản lý vòng đời dịch vụ và cung cấp điểm truy cập Use Cases trung tâm cho WinForms Admin.
/// Tự động kết nối cơ sở dữ liệu qua ADO.NET thuần (DatabaseHelper).
/// </summary>
public class AdminServiceScope
{
    private static AdminServiceScope? _instance;
    private static readonly object _lock = new();

    public static AdminServiceScope Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new AdminServiceScope();
                }
            }
            return _instance;
        }
    }

    public ISanPhamRepository SanPhamRepo { get; }
    public INhomSanPhamRepository NhomSanPhamRepo { get; }
    public IDanhMucRepository DanhMucRepo { get; }
    public IHoaDonRepository HoaDonRepo { get; }
    public IGioHangRepository GioHangRepo { get; }
    public IBaoHanhRepository BaoHanhRepo { get; }
    public INhanVienRepository NhanVienRepo { get; }
    public IKhachHangRepository KhachHangRepo { get; }
    public IVaiTroRepository VaiTroRepo { get; }
    public IThongKeRepository ThongKeRepo { get; }
    public IPhieuNhapKhoRepository PhieuNhapKhoRepo { get; }
    public IPhieuKiemKeRepository PhieuKiemKeRepo { get; }
    public INguoiDungRepository NguoiDungRepo { get; }

    public QuanLySanPhamUseCase QuanLySanPham { get; }
    public QuanLyNhomSanPhamUseCase QuanLyNhomSanPham { get; }
    public XacNhanNhapKhoUseCase XacNhanNhapKho { get; }
    public XuLyDonHangUseCase XuLyDonHang { get; }
    public DatHangUseCase DatHang { get; }
    public BanHangOfflineUseCase BanHangOffline { get; }
    public TiepNhanBaoHanhUseCase TiepNhanBaoHanh { get; }
    public XuLyBaoHanhUseCase XuLyBaoHanh { get; }
    public QuanLyNhanVienUseCase QuanLyNhanVien { get; }
    public QuanLyKhachHangUseCase QuanLyKhachHang { get; }
    public ThongKeDoanhThuUseCase ThongKeDoanhThu { get; }
    public ThongKeKhoHangUseCase ThongKeKhoHang { get; }
    public QuanLyKiemKeUseCase QuanLyKiemKe { get; }

    /// <summary>
    /// Người dùng / Nhân viên đang đăng nhập vào phần mềm Admin WinForms phục vụ phân quyền.
    /// </summary>
    public NguoiDung? CurrentUser { get; set; }
    public NhanVien? CurrentStaff { get; set; }

    public bool IsDatabaseConnected { get; private set; }

    /// <summary>
    /// Bộ ghi nhật ký thích ứng cho ứng dụng quản trị WinForms.
    /// </summary>
    public IAppLogger Logger { get; }

    private AdminServiceScope()
    {
        var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                      ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                      ?? (Debugger.IsAttached ? "Development" : "Production");

        var localLogDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BlinkBlink",
            "logs");

        Logger = new AdaptiveFileLogger(
            new AppLogContext
            {
                Channel = "WinForms",
                EnvironmentName = envName,
                DataMode = "SqlServer"
            },
            preferredDirectory: localLogDir,
            filePrefix: "admin",
            writeToConsole: false);

        // Danh sách các chuỗi kết nối máy chủ SQL Server theo thứ tự ưu tiên
        var candidateConnections = new[]
        {
            "Server=.;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;",
            "Server=localhost;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;",
            "Server=.\\MSSQL;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;",
            "Server=(localdb)\\mssqllocaldb;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;"
        };

        DatabaseHelper? dbHelper = null;
        foreach (var connStr in candidateConnections)
        {
            try
            {
                var helper = new DatabaseHelper(connStr, Logger);
                if (helper.TestConnection())
                {
                    dbHelper = helper;
                    IsDatabaseConnected = true;
                    Logger.LogInfo(nameof(AdminServiceScope), $"WinForms kết nối thành công SQL Server: {AdaptiveFileLogger.MaskConnectionString(connStr)}");
                    break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(nameof(AdminServiceScope), $"Thử kết nối SQL Server thất bại ({AdaptiveFileLogger.MaskConnectionString(connStr)}).", ErrorCodes.DatabaseUnavailable, ex);
            }
        }

        // Bước 2: Nếu chưa có database nào, kích hoạt cơ chế tự động khởi tạo CSDL qua SQL Server đang chạy
        if (dbHelper == null)
        {
            foreach (var connStr in candidateConnections)
            {
                try
                {
                    if (DatabaseHelper.TryProvisionDatabase(connStr, out var provisionedHelper, Logger) && provisionedHelper != null)
                    {
                        dbHelper = provisionedHelper;
                        IsDatabaseConnected = true;
                        Logger.LogInfo(nameof(AdminServiceScope), $"WinForms tự động khởi tạo thành công CSDL tại: {AdaptiveFileLogger.MaskConnectionString(connStr)}");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(nameof(AdminServiceScope), $"Tự động khởi tạo CSDL thất bại ({AdaptiveFileLogger.MaskConnectionString(connStr)}).", ErrorCodes.DatabaseUnavailable, ex);
                }
            }
        }

        if (IsDatabaseConnected && dbHelper != null)
        {
            // Tự động kiểm tra và nâng cấp schema nếu database hiện tại thiếu cột
            dbHelper.EnsureSchemaCompatibility();

            SanPhamRepo = new SanPhamRepository(dbHelper);
            NhomSanPhamRepo = new NhomSanPhamRepository(dbHelper);
            DanhMucRepo = new DanhMucRepository(dbHelper);
            HoaDonRepo = new HoaDonRepository(dbHelper);
            GioHangRepo = new GioHangRepository(dbHelper);
            BaoHanhRepo = new BaoHanhRepository(dbHelper);
            NhanVienRepo = new NhanVienRepository(dbHelper);
            KhachHangRepo = new KhachHangRepository(dbHelper);
            VaiTroRepo = new VaiTroRepository(dbHelper);
            ThongKeRepo = new ThongKeRepository(dbHelper);
            PhieuNhapKhoRepo = new PhieuNhapKhoRepositoryImpl(dbHelper);
            PhieuKiemKeRepo = new PhieuKiemKeRepository(dbHelper);
            NguoiDungRepo = new NguoiDungRepository(dbHelper);
        }
        else
        {
            // Dự phòng kho dữ liệu nội bộ trong bộ nhớ khi chưa cấu hình máy chủ SQL Server
            Logger.DefaultContext.DataMode = "InMemory";
            Logger.LogWarning(
                nameof(AdminServiceScope),
                "Không tìm thấy máy chủ SQL Server khả dụng. WinForms chuyển sang chế độ dữ liệu bộ nhớ tạm InMemoryDataContext.",
                ErrorCodes.DatabaseUnavailable);

            var mock = new InMemoryDataContext();
            mock.SeedInitialLuxuryData();

            SanPhamRepo = mock.SanPhamRepo;
            NhomSanPhamRepo = mock.NhomSanPhamRepo;
            DanhMucRepo = mock.DanhMucRepo;
            HoaDonRepo = mock.HoaDonRepo;
            GioHangRepo = mock.GioHangRepo;
            BaoHanhRepo = mock.BaoHanhRepo;
            NhanVienRepo = mock.NhanVienRepo;
            KhachHangRepo = mock.KhachHangRepo;
            VaiTroRepo = mock.VaiTroRepo;
            ThongKeRepo = mock.ThongKeRepo;
            PhieuNhapKhoRepo = mock.PhieuNhapKhoRepo;
            PhieuKiemKeRepo = mock.PhieuKiemKeRepo;
            NguoiDungRepo = mock.NguoiDungRepo;
        }

        // Khởi tạo tất cả Use Cases theo Clean Architecture
        QuanLySanPham = new QuanLySanPhamUseCase(SanPhamRepo, DanhMucRepo);
        QuanLyNhomSanPham = new QuanLyNhomSanPhamUseCase(NhomSanPhamRepo, SanPhamRepo);
        XacNhanNhapKho = new XacNhanNhapKhoUseCase(PhieuNhapKhoRepo, SanPhamRepo);
        XuLyDonHang = new XuLyDonHangUseCase(HoaDonRepo, SanPhamRepo);
        DatHang = new DatHangUseCase(HoaDonRepo, GioHangRepo, SanPhamRepo);
        BanHangOffline = new BanHangOfflineUseCase(HoaDonRepo, SanPhamRepo, KhachHangRepo);
        TiepNhanBaoHanh = new TiepNhanBaoHanhUseCase(BaoHanhRepo, SanPhamRepo, KhachHangRepo);
        XuLyBaoHanh = new XuLyBaoHanhUseCase(BaoHanhRepo, SanPhamRepo);
        QuanLyNhanVien = new QuanLyNhanVienUseCase(NhanVienRepo, VaiTroRepo);
        QuanLyKhachHang = new QuanLyKhachHangUseCase(KhachHangRepo);
        ThongKeDoanhThu = new ThongKeDoanhThuUseCase(ThongKeRepo);
        ThongKeKhoHang = new ThongKeKhoHangUseCase(ThongKeRepo);
        QuanLyKiemKe = new QuanLyKiemKeUseCase(PhieuKiemKeRepo, SanPhamRepo);
    }
}
