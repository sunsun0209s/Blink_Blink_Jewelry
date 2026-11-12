using System;
using System.Linq;
using System.Threading.Tasks;
using BlinkBlink.Application.DTOs.Catalog;
using BlinkBlink.Application.DTOs.Warranty;
using BlinkBlink.Application.Services.Catalog;
using BlinkBlink.Application.Services.Warranty;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Infrastructure.Data;
using BlinkBlink.Infrastructure.Repositories;
using Xunit;

namespace BlinkBlink.IntegrationTests;

/// <summary>
/// Kiểm thử tích hợp toàn diện dữ liệu và các luồng nghiệp vụ trên CSDL SQL Server thật (BlinkBlinkJewelryDb).
/// Bao gồm: Chu trình bảo hành đầy đủ, ánh xạ khóa ngoại thành viên/khách vãng lai, đồng bộ tên nhóm sản phẩm.
/// </summary>
public class DatabaseWorkflowIntegrationTests
{
    private const string ConnectionString = "Server=.;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

    private readonly DatabaseHelper _dbHelper;
    private readonly bool _isDbAvailable;

    public DatabaseWorkflowIntegrationTests()
    {
        _dbHelper = new DatabaseHelper(ConnectionString);
        _isDbAvailable = _dbHelper.TestConnection();
    }

    [Fact]
    public async Task KiemTra_ChuTrinhBaoHanhDayDu_KhachThanhVien_Va_KhachVangLai_TrenCSDL()
    {
        if (!_isDbAvailable) return;

        var baoHanhRepo = new BaoHanhRepository(_dbHelper);
        var sanPhamRepo = new SanPhamRepository(_dbHelper);
        var khachHangRepo = new KhachHangRepository(_dbHelper);

        var tiepNhanUseCase = new TiepNhanBaoHanhUseCase(baoHanhRepo, sanPhamRepo, khachHangRepo);
        var xuLyUseCase = new XuLyBaoHanhUseCase(baoHanhRepo, sanPhamRepo);

        // 1. Lấy sản phẩm thực tế có trong CSDL để tạo phiếu bảo hành
        var allProducts = sanPhamRepo.LayTatCa();
        Assert.NotEmpty(allProducts);
        var testSp = allProducts.First(p => p.TrangThai != "DangBaoHanh");
        var originalStatus = testSp.TrangThai;

        // 2. Test Tạo phiếu bảo hành cho Khách hàng thành viên (KH001)
        var reqThanhVien = new TaoBaoHanhDTO
        {
            MaKhachHang = "KH001", // Mã khách hàng nghiệp vụ
            MaSanPham = testSp.MaSanPham,
            LyDoBaoHanh = "Gia cố chấu kim cương và làm sáng",
            TinhTrangTiepNhan = "Xước dăm nhẹ quanh thân nhẫn",
            GhiChu = "Khách hàng VIP cần xử lý kỹ",
            ChiPhiBaoHanh = 0m,
            NgayHenTra = DateTime.UtcNow.AddDays(2),
            SoDienThoai = "0912345678"
        };

        var okThanhVien = await tiepNhanUseCase.ExecuteAsync(reqThanhVien);
        Assert.True(okThanhVien, "Tạo phiếu bảo hành cho khách thành viên phải thành công.");

        // Lấy phiếu vừa tạo từ CSDL
        var listTickets = baoHanhRepo.LayDanhSachPhieu();
        var ticketTv = listTickets.FirstOrDefault(t => t.MaSanPham == testSp.MaSanPham && t.TrangThaiXuLy == "TiepNhan");
        Assert.NotNull(ticketTv);
        Assert.Equal("TiepNhan", ticketTv.TrangThaiXuLy);
        // Xác minh mã khách hàng trong DB được lưu thành công là USER_KH001 (khớp FK dbo.NguoiDung)
        Assert.Equal("USER_KH001", ticketTv.MaKhachHang);

        // Kiểm tra sản phẩm được cập nhật trạng thái DangBaoHanh
        var spSauTiepNhan = await sanPhamRepo.TimTheoIdAsync(testSp.MaSanPham);
        Assert.NotNull(spSauTiepNhan);
        Assert.Equal("DangBaoHanh", spSauTiepNhan.TrangThai);

        // 3. Tiến độ: Tiếp Nhận -> Đang Xử Lý (Kỹ thuật viên bắt đầu sửa)
        var okDangXuLy = xuLyUseCase.CapNhatTienDo(ticketTv.SoPhieuBH, "DangXuLy");
        Assert.True(okDangXuLy);
        var ticketDangXuLy = baoHanhRepo.TimTheoSoPhieu(ticketTv.SoPhieuBH);
        Assert.NotNull(ticketDangXuLy);
        Assert.Equal("DangXuLy", ticketDangXuLy.TrangThaiXuLy);

        // 4. Tiến độ: Đang Xử Lý -> Đã Xử Lý (Quyết toán chi phí thực tế)
        var okQuyetToan = xuLyUseCase.QuyetToanBaoHanh(ticketTv.SoPhieuBH, 180000m);
        Assert.True(okQuyetToan);
        var ticketDaXuLy = baoHanhRepo.TimTheoSoPhieu(ticketTv.SoPhieuBH);
        Assert.NotNull(ticketDaXuLy);
        Assert.Equal("DaXuLy", ticketDaXuLy.TrangThaiXuLy);
        Assert.Equal(180000m, ticketDaXuLy.ChiPhiThucTe);

        // 5. Tiến độ: Đã Xử Lý -> Đã Trả Khách (Hoàn tất dịch vụ & trả trang sức)
        var okHoanTat = xuLyUseCase.HoanTatBaoHanh(ticketTv.SoPhieuBH);
        Assert.True(okHoanTat);
        var ticketHoanTat = baoHanhRepo.TimTheoSoPhieu(ticketTv.SoPhieuBH);
        Assert.NotNull(ticketHoanTat);
        Assert.Equal("DaTraKhach", ticketHoanTat.TrangThaiXuLy);

        // Sản phẩm được chuyển trạng thái về DaBan
        var spSauHoanTat = await sanPhamRepo.TimTheoIdAsync(testSp.MaSanPham);
        Assert.NotNull(spSauHoanTat);
        Assert.Equal("DaBan", spSauHoanTat.TrangThai);

        // 6. Test Tạo phiếu cho Khách vãng lai (MaKhachHang = "0")
        var reqVangLai = new TaoBaoHanhDTO
        {
            MaKhachHang = "0",
            MaSanPham = testSp.MaSanPham,
            LyDoBaoHanh = "Chỉnh ni nhẫn vãng lai",
            ChiPhiBaoHanh = 50000m,
            NgayHenTra = DateTime.UtcNow.AddDays(1),
            SoDienThoai = "0988776655"
        };
        var okVangLai = await tiepNhanUseCase.ExecuteAsync(reqVangLai);
        Assert.True(okVangLai, "Tạo phiếu cho khách vãng lai phải thành công.");

        var ticketVl = baoHanhRepo.LayDanhSachPhieu().FirstOrDefault(t => t.MaKhachHang == "0" && t.SoDienThoai == "0988776655");
        Assert.NotNull(ticketVl);
        Assert.Equal("0", ticketVl.MaKhachHang);

        // Dọn dẹp dữ liệu test để không làm ô nhiễm database
        _dbHelper.ExecuteNonQuery("DELETE FROM dbo.PhieuBaoHanh WHERE SoPhieuBH IN (@p1, @p2)",
            new Microsoft.Data.SqlClient.SqlParameter("@p1", ticketTv.SoPhieuBH),
            new Microsoft.Data.SqlClient.SqlParameter("@p2", ticketVl.SoPhieuBH));

        // Phục hồi trạng thái ban đầu của sản phẩm
        testSp.CapNhatTrangThai(originalStatus);
        await sanPhamRepo.LuuSanPhamAsync(testSp);
    }

    [Fact]
    public async Task KiemTra_NghiepVuNhomSanPham_CapNhatTenDongBo_Va_GiuNguyenTenGoc_TrenCSDL()
    {
        if (!_isDbAvailable) return;

        var nhomRepo = new NhomSanPhamRepository(_dbHelper);
        var sanPhamRepo = new SanPhamRepository(_dbHelper);
        var useCase = new QuanLyNhomSanPhamUseCase(nhomRepo, sanPhamRepo);

        // 1. Tạo nhóm sản phẩm kiểm thử
        var maNhomTest = $"NHOM_TEST_{DateTime.UtcNow.Ticks % 100000}";
        var tenNhomBanDau = "Nhẫn Cưới Bạch Kim Hoàng Gia";
        var nhom = new NhomSanPham(maNhomTest, tenNhomBanDau, "Nhóm kiểm thử tự động");
        nhomRepo.LuuNhom(nhom);

        // 2. Tạo một sản phẩm mẫu độc lập để thêm vào nhóm
        var maSpTest = $"SP_TEST_{DateTime.UtcNow.Ticks % 100000}";
        var tenSpGoc = "Nhẫn Bạch Kim Đính Kim Cương Tự Nhiên Size 14";
        var spTest = new SanPhamTrangSuc(maSpTest, tenSpGoc, "DM01", "Bạch kim 750", "Size 14", 12000000m, 18000000m, "SanSang");
        await sanPhamRepo.LuuSanPhamAsync(spTest);

        try
        {
            // 3. Thêm sản phẩm vào nhóm -> BẮT BUỘC giữ nguyên tên gốc của sản phẩm (theo dòng 6 CONGVIECTIEPTHEO.md)
            var okThem = useCase.ThemSanPhamVaoNhom(maNhomTest, maSpTest, out var err);
            Assert.True(okThem, $"Thêm sản phẩm vào nhóm phải thành công: {err}");

            var spSauThem = await sanPhamRepo.TimTheoIdAsync(maSpTest);
            Assert.NotNull(spSauThem);
            Assert.Equal(maNhomTest, spSauThem.MaNhom);
            Assert.Equal(tenSpGoc, spSauThem.TenSanPham); // Tên gốc KHÔNG được thay đổi

            // 4. Sửa tên nhóm -> Tùy chỉnh tên nhóm độc lập, giữ nguyên tên riêng của sản phẩm trong nhóm
            var tenNhomMoi = "Nhẫn Cưới Kim Cương Hoàng Gia Diamond Luxury";
            var okSuaTen = useCase.CapNhatTenNhom(maNhomTest, tenNhomMoi);
            Assert.True(okSuaTen, "Sửa tên nhóm phải thành công.");

            var nhomDb = nhomRepo.TimTheoId(maNhomTest);
            Assert.NotNull(nhomDb);
            Assert.Equal(tenNhomMoi, nhomDb.TenNhom);

            var spSauSuaTen = await sanPhamRepo.TimTheoIdAsync(maSpTest);
            Assert.NotNull(spSauSuaTen);
            Assert.Equal(tenSpGoc, spSauSuaTen.TenSanPham); // Tên sản phẩm được bảo toàn độc lập, không bị ghi đè

            // 5. Loại sản phẩm khỏi nhóm -> Mã nhóm trở về "0"
            var okLoai = useCase.LoaiSanPhamKhoiNhom(maSpTest);
            Assert.True(okLoai);
            var spSauLoai = await sanPhamRepo.TimTheoIdAsync(maSpTest);
            Assert.NotNull(spSauLoai);
            Assert.Equal("0", spSauLoai.MaNhom);
        }
        finally
        {
            // Dọn dẹp dữ liệu test
            useCase.XoaNhom(maNhomTest);
            _dbHelper.ExecuteNonQuery("DELETE FROM dbo.SanPhamTrangSuc WHERE MaSanPham = @p1",
                new Microsoft.Data.SqlClient.SqlParameter("@p1", maSpTest));
        }
    }

    [Fact]
    public void KiemTra_TruyVanSanPham_NapTenNhom_TuDatabase()
    {
        if (!_isDbAvailable) return;

        var sanPhamRepo = new SanPhamRepository(_dbHelper);
        var products = sanPhamRepo.LayTatCa();
        Assert.NotEmpty(products);

        // Với các sản phẩm đã thuộc NHOM01 (trong SeedData)
        var groupedProducts = products.Where(p => p.MaNhom == "NHOM01").ToList();
        if (groupedProducts.Count > 0)
        {
            foreach (var sp in groupedProducts)
            {
                Assert.NotNull(sp.NhomSanPham);
                Assert.False(string.IsNullOrWhiteSpace(sp.NhomSanPham.TenNhom), $"Sản phẩm {sp.MaSanPham} phải nạp được TenNhom.");
            }
        }
    }
}
