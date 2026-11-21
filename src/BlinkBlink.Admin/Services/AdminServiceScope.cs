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

    public ISanPhamRepository SanPhamRepo { get; private set; } = null!;
    public INhomSanPhamRepository NhomSanPhamRepo { get; private set; } = null!;
    public IDanhMucRepository DanhMucRepo { get; private set; } = null!;
    public IHoaDonRepository HoaDonRepo { get; private set; } = null!;
    public IGioHangRepository GioHangRepo { get; private set; } = null!;
    public IBaoHanhRepository BaoHanhRepo { get; private set; } = null!;
    public INhanVienRepository NhanVienRepo { get; private set; } = null!;
    public IKhachHangRepository KhachHangRepo { get; private set; } = null!;
    public IVaiTroRepository VaiTroRepo { get; private set; } = null!;
    public IThongKeRepository ThongKeRepo { get; private set; } = null!;
    public IPhieuNhapKhoRepository PhieuNhapKhoRepo { get; private set; } = null!;
    public IPhieuKiemKeRepository PhieuKiemKeRepo { get; private set; } = null!;
    public INguoiDungRepository NguoiDungRepo { get; private set; } = null!;

    public QuanLySanPhamUseCase QuanLySanPham { get; private set; } = null!;
    public QuanLyNhomSanPhamUseCase QuanLyNhomSanPham { get; private set; } = null!;
    public XacNhanNhapKhoUseCase XacNhanNhapKho { get; private set; } = null!;
    public XuLyDonHangUseCase XuLyDonHang { get; private set; } = null!;
    public DatHangUseCase DatHang { get; private set; } = null!;
    public BanHangOfflineUseCase BanHangOffline { get; private set; } = null!;
    public TiepNhanBaoHanhUseCase TiepNhanBaoHanh { get; private set; } = null!;
    public XuLyBaoHanhUseCase XuLyBaoHanh { get; private set; } = null!;
    public QuanLyNhanVienUseCase QuanLyNhanVien { get; private set; } = null!;
    public QuanLyKhachHangUseCase QuanLyKhachHang { get; private set; } = null!;
    public ThongKeDoanhThuUseCase ThongKeDoanhThu { get; private set; } = null!;
    public ThongKeKhoHangUseCase ThongKeKhoHang { get; private set; } = null!;
    public QuanLyKiemKeUseCase QuanLyKiemKe { get; private set; } = null!;

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

        TryConnectOrProvision();
    }

    /// <summary>
    /// Thử dò tìm hoặc tự động khởi tạo kết nối SQL Server khả dụng.
    /// Trả về true nếu kết nối thành công, false nếu không tìm thấy máy chủ CSDL.
    /// </summary>
    public bool TryConnectOrProvision()
    {
        var candidateConnections = GetCandidateConnectionStrings();
        DatabaseHelper? dbHelper = null;

        // Bước 1: Thử các chuỗi kết nối đã tồn tại
        foreach (var connStr in candidateConnections)
        {
            // Tối ưu tốc độ khởi động: Nếu là chuỗi kết nối Docker trên cổng 1433, kiểm tra nhanh cổng bằng Socket (timeout 150ms).
            if (connStr.Contains(",1433") && !IsPortOpen("127.0.0.1", 1433, 150))
            {
                continue;
            }

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
                if (connStr.Contains(",1433") && !IsPortOpen("127.0.0.1", 1433, 150))
                {
                    continue;
                }

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
            InitializeRepositoriesAndUseCases(dbHelper);
            return true;
        }

        // Khi hoàn toàn không có máy chủ CSDL khả dụng:
        // Đặt trạng thái ngắt kết nối và gán các Repository an toàn với fallback connection để tránh NullReferenceException
        IsDatabaseConnected = false;
        Logger.LogError(
            nameof(AdminServiceScope),
            "Không tìm thấy máy chủ SQL Server khả dụng. WinForms chuyển sang trạng thái ngắt kết nối (Disconnected).",
            ErrorCodes.DatabaseUnavailable);

        var fallbackConn = candidateConnections.FirstOrDefault() ?? "Server=localhost;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;TrustServerCertificate=True;";
        var fallbackHelper = new DatabaseHelper(fallbackConn, Logger);
        InitializeRepositoriesAndUseCases(fallbackHelper);
        return false;
    }

    /// <summary>
    /// Khởi tạo các Repository và Use Cases theo chuẩn Clean Architecture
    /// </summary>
    private void InitializeRepositoriesAndUseCases(DatabaseHelper dbHelper)
    {
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

    /// <summary>
    /// Danh sách các chuỗi kết nối máy chủ SQL Server theo thứ tự ưu tiên
    /// </summary>
    private static string[] GetCandidateConnectionStrings()
    {
        var customConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                         ?? Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection");

        var candidateList = new List<string>();
        if (!string.IsNullOrWhiteSpace(customConn))
        {
            candidateList.Add(customConn.Trim());
        }
        candidateList.AddRange(new[]
        {
            "Server=localhost,1433;Database=BlinkBlinkJewelryDb;User Id=sa;Password=BlinkBlink@Dev2026;TrustServerCertificate=True;MultipleActiveResultSets=true;Connect Timeout=4;",
            "Server=127.0.0.1,1433;Database=BlinkBlinkJewelryDb;User Id=sa;Password=BlinkBlink@Dev2026;TrustServerCertificate=True;MultipleActiveResultSets=true;Connect Timeout=4;",
            "Server=.\\MSSQL;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;",
            "Server=.\\SQLEXPRESS;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;",
            "Server=localhost\\SQLEXPRESS;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;",
            "Server=.;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;",
            "Server=localhost;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;",
            "Server=(localdb)\\mssqllocaldb;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=3;"
        });
        return candidateList.Distinct().ToArray();
    }

    /// <summary>
    /// Kiểm tra nhanh trạng thái cổng TCP (Socket thăm dò với timeout siêu ngắn) để phát hiện Docker có đang chạy hay không.
    /// Giúp loại bỏ hoàn toàn độ trễ 4-8 giây khi Docker chưa bật.
    /// </summary>
    private static bool IsPortOpen(string host, int port, int timeoutMs = 150)
    {
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            var result = client.BeginConnect(host, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeoutMs));
            if (!success) return false;
            client.EndConnect(result);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
