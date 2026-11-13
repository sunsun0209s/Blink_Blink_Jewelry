using System;
using System.Collections.Generic;
using System.Linq;
using BlinkBlink.Application.DTOs.Identity;
using BlinkBlink.Application.DTOs.Warranty;
using BlinkBlink.Application.Services.Identity;
using BlinkBlink.Application.Services.Warranty;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Entities.Orders;
using BlinkBlink.Domain.Entities.Warranty;
using BlinkBlink.Domain.Interfaces;
using Xunit;

namespace BlinkBlink.UnitTests.Services.Warranty;

/// <summary>
/// Bộ kiểm thử xác thực vòng đời bảo hành chuẩn (TiepNhan -> DangXuLy -> DaXuLy -> DaTraKhach),
/// tự động liên kết hóa đơn khách vãng lai khi đăng ký và các ràng buộc nghiệp vụ.
/// </summary>
public class WarrantyWorkflowAndPosTests
{
    [Fact]
    public void VongDoiBaoHanh_TiepNhan_DangXuLy_DaXuLy_DaTraKhach_ChuanQuyTrinh()
    {
        // 1. Khởi tạo dữ liệu mẫu
        var phieu = new PhieuBaoHanh(
            soPhieuBH: "BH_TEST_001",
            maKhachHang: "KH001",
            maSanPham: "SP001",
            lyDoBaoHanh: "Gắn đá chủ",
            chiPhiUocTinh: 0m,
            trangThaiXuLy: "TiepNhan",
            ngayTiepNhan: DateTime.UtcNow,
            ngayHenTra: DateTime.UtcNow.AddDays(3),
            chiPhiThucTe: 0m,
            tinhTrangTiepNhan: "Chấu lỏng, rơi đá",
            ghiChu: "Khách cần gấp"
        );

        Assert.Equal("TiepNhan", phieu.TrangThaiXuLy);
        Assert.Equal("Chấu lỏng, rơi đá", phieu.TinhTrangTiepNhan);
        Assert.Equal("Khách cần gấp", phieu.GhiChu);

        // 2. Chuyển sang DangXuLy
        var canDangXuLy = phieu.ChuyenSangDangXuLy();
        Assert.True(canDangXuLy);
        Assert.Equal("DangXuLy", phieu.TrangThaiXuLy);

        // 3. Quyết toán chi phí thực tế sang DaXuLy
        var canQuyetToan = phieu.QuyetToan(350000m);
        Assert.True(canQuyetToan);
        Assert.Equal("DaXuLy", phieu.TrangThaiXuLy);
        Assert.Equal(350000m, phieu.ChiPhiThucTe);

        // 4. Hoàn tất & Trả khách sang DaTraKhach
        var canTraKhach = phieu.HoanTatBaoHanh();
        Assert.True(canTraKhach);
        Assert.Equal("DaTraKhach", phieu.TrangThaiXuLy);
    }

    [Fact]
    public void HuyPhieuBaoHanh_ThanhCong_KhiChuaHoanTat()
    {
        var phieu = new PhieuBaoHanh(
            soPhieuBH: "BH_TEST_HUY",
            maKhachHang: "KH001",
            maSanPham: "SP001",
            lyDoBaoHanh: "Đánh bóng",
            chiPhiUocTinh: 50000m,
            trangThaiXuLy: "TiepNhan"
        );

        var canHuy = phieu.HuyPhieu();
        Assert.True(canHuy);
        Assert.Equal("DaHuy", phieu.TrangThaiXuLy);

        // Không thể hủy lại khi đã hủy
        Assert.False(phieu.HuyPhieu());
    }

    [Fact]
    public void DangKyKhachHang_TuDongCapNhatMaKhachHang_ChoCacDonHangOfflineVangLai()
    {
        // Giả lập Repository
        var fakeKhRepo = new FakeKhachHangRepository();
        var fakeHdRepo = new FakeHoaDonRepository();

        // Khách vãng lai mua hàng trước đó với SĐT nhưng mã KH = "0"
        var hd1 = new HoaDon(
            maHoaDon: "HD_OFFLINE_001",
            ngayLap: DateTime.UtcNow.AddDays(-1),
            maKhachHang: "0",
            diaChiGiaoHang: "Tại quầy",
            soDienThoaiNhan: "0988776655",
            phuongThucThanhToan: "TienMat",
            trangThaiDonHang: "DaGiaoHang",
            ghiChu: "Khách vãng lai mua tại quầy"
        );
        fakeHdRepo.Data.Add(hd1);

        var useCase = new DangKyKhachHangUseCase(fakeKhRepo, fakeHdRepo);

        var dto = new DangKyKhachHangDTO
        {
            HoTen = "Lê Thị Khách Quen",
            SoDienThoai = "0988776655",
            TenDangNhap = "lethikhachquen",
            MatKhau = "123456789"
        };

        var result = useCase.Execute(dto);

        Assert.True(result);
        Assert.Single(fakeKhRepo.Data);
        var khMoi = fakeKhRepo.Data.First();

        // Kiểm tra hóa đơn cũ mã "0" đã được cập nhật thành mã khách hàng mới đăng ký
        Assert.Equal(khMoi.MaKhachHang, hd1.MaKhachHang);
        Assert.NotEqual("0", hd1.MaKhachHang);
    }

    [Fact]
    public void SanPhamTrangSuc_KiemTraKinhDoanhVaTonKho()
    {
        var spHopLe = new SanPhamTrangSuc("SP_OK", "Nhẫn Vàng", "DM01", "Vàng 18K", "Size 12", 10000000m, 15000000m, "SanSang")
        {
            SoLuongTon = 5
        };

        var spHetHang = new SanPhamTrangSuc("SP_HET", "Nhẫn Bạc", "DM01", "Bạc 925", "Size 14", 500000m, 800000m, "HetHang")
        {
            SoLuongTon = 0
        };

        Assert.True(spHopLe.KiemTraTonKho() && spHopLe.SoLuongTon > 0);
        Assert.False(spHetHang.KiemTraTonKho() && spHetHang.SoLuongTon > 0);
    }

    // Các lớp Fake Repository nội bộ phục vụ kiểm thử
    private class FakeKhachHangRepository : IKhachHangRepository
    {
        public List<KhachHang> Data { get; } = new();
        public void LuuKhachHang(KhachHang kh) => Data.Add(kh);
        public KhachHang? TimTheoMa(string maKH) => Data.FirstOrDefault(k => k.MaKhachHang == maKH);
        public KhachHang? TimTheoSoDienThoai(string sdt) => Data.FirstOrDefault(k => k.SoDienThoai == sdt);
        public List<KhachHang> LayTatCa() => Data;
        public bool KiemTraCoTheXoa(string maKH) => true;
        public bool XoaKhachHang(string maKH) => true;
    }

    private class FakeHoaDonRepository : IHoaDonRepository
    {
        public List<HoaDon> Data { get; } = new();

        public void CapNhatKhachHangTheoSoDienThoai(string soDienThoai, string maKhachHangMoi)
        {
            foreach (var hd in Data.Where(h => h.SoDienThoaiNhan == soDienThoai && (h.MaKhachHang == "0" || h.MaKhachHang == "KH_0")))
            {
                hd.MaKhachHang = maKhachHangMoi;
            }
        }

        public void LuuHoaDon(HoaDon hd) => Data.Add(hd);
        public System.Threading.Tasks.Task LuuHoaDonAsync(HoaDon hd, System.Threading.CancellationToken ct = default) { LuuHoaDon(hd); return System.Threading.Tasks.Task.CompletedTask; }
        public HoaDon? TimTheoMa(string maHD) => Data.FirstOrDefault(h => h.MaHoaDon == maHD);
        public System.Threading.Tasks.Task<HoaDon?> TimTheoMaAsync(string maHD, System.Threading.CancellationToken ct = default) => System.Threading.Tasks.Task.FromResult(TimTheoMa(maHD));
        public List<HoaDon> LayTheoKhachHang(string maKH) => Data.Where(h => h.MaKhachHang == maKH).ToList();
        public System.Threading.Tasks.Task<List<HoaDon>> LayTheoKhachHangAsync(string maKH, System.Threading.CancellationToken ct = default) => System.Threading.Tasks.Task.FromResult(LayTheoKhachHang(maKH));
        public List<HoaDon> LayTatCa() => Data;
        public System.Threading.Tasks.Task<List<HoaDon>> LayTatCaAsync(System.Threading.CancellationToken ct = default) => System.Threading.Tasks.Task.FromResult(LayTatCa());
        public List<HoaDon> LayDanhSachDonHang() => Data;
        public System.Threading.Tasks.Task<List<HoaDon>> LayDanhSachDonHangAsync(System.Threading.CancellationToken ct = default) => System.Threading.Tasks.Task.FromResult(Data);
        public void CapNhatHoaDon(HoaDon hd) { }
        public System.Threading.Tasks.Task CapNhatHoaDonAsync(HoaDon hd, System.Threading.CancellationToken ct = default) => System.Threading.Tasks.Task.CompletedTask;
    }
}
