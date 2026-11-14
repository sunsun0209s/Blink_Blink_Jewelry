using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using BlinkBlink.Application.DTOs.Inventory;
using BlinkBlink.Application.Services.Inventory;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Inventory;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.UnitTests.Services.Inventory;

/// <summary>
/// Kiểm thử toàn diện luồng nghiệp vụ Kiểm kê kho hàng (QuanLyKiemKeUseCase)
/// và Vô hiệu hóa phiếu nhập kho (XacNhanNhapKhoUseCase).
/// Kiểm chứng các quy tắc chống chồng chéo (Anti-Conflict) và hoàn tác tồn kho theo Delta.
/// </summary>
public class StockAuditWorkflowTests
{
    private class FakeSanPhamRepository : ISanPhamRepository
    {
        public readonly Dictionary<string, SanPhamTrangSuc> Data = new();

        public void LuuSanPham(SanPhamTrangSuc sp) => Data[sp.MaSanPham] = sp;
        public SanPhamTrangSuc? TimTheoId(string maSanPham) => Data.TryGetValue(maSanPham, out var sp) ? sp : null;
        public List<SanPhamTrangSuc> LayTatCa() => Data.Values.ToList();
        public List<SanPhamTrangSuc> LayTheoNhom(string maNhom) => Data.Values.Where(s => s.MaNhom == maNhom).ToList();

        public Task LuuSanPhamAsync(SanPhamTrangSuc sp, CancellationToken ct = default) { LuuSanPham(sp); return Task.CompletedTask; }
        public Task<SanPhamTrangSuc?> TimTheoIdAsync(string maSanPham, CancellationToken ct = default) => Task.FromResult(TimTheoId(maSanPham));
        public Task<List<SanPhamTrangSuc>> LayTatCaAsync(CancellationToken ct = default) => Task.FromResult(LayTatCa());
        public Task<List<SanPhamTrangSuc>> LayTheoNhomAsync(string maNhom, CancellationToken ct = default) => Task.FromResult(LayTheoNhom(maNhom));
    }

    private class FakePhieuKiemKeRepository : IPhieuKiemKeRepository
    {
        public readonly Dictionary<string, PhieuKiemKe> Data = new();

        public void LuuPhieuKiemKe(PhieuKiemKe phieu) => Data[phieu.MaPhieuKiemKe] = phieu;
        public PhieuKiemKe? TimTheoId(string maPhieu) => Data.TryGetValue(maPhieu, out var p) ? p : null;
        public List<PhieuKiemKe> LayTatCa() => Data.Values.OrderByDescending(p => p.NgayKiemKe).ToList();

        public void XoaPhieuKiemKe(string maPhieu)
        {
            if (Data.TryGetValue(maPhieu, out var p) && string.Equals(p.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
            {
                Data.Remove(maPhieu);
            }
        }

        public void CapNhatTrangThai(string maPhieu, string trangThaiMoi, DateTime? ngayDuyet = null)
        {
            if (Data.TryGetValue(maPhieu, out var p))
            {
                p.TrangThai = trangThaiMoi;
                p.TrangThaiDuyet = string.Equals(trangThaiMoi, "DaDuyet", StringComparison.OrdinalIgnoreCase);
                if (ngayDuyet.HasValue) p.NgayDuyet = ngayDuyet.Value;
            }
        }

        public Task LuuPhieuKiemKeAsync(PhieuKiemKe phieu, CancellationToken ct = default) { LuuPhieuKiemKe(phieu); return Task.CompletedTask; }
        public Task<PhieuKiemKe?> TimTheoIdAsync(string maPhieu, CancellationToken ct = default) => Task.FromResult(TimTheoId(maPhieu));
    }

    private class FakePhieuNhapKhoRepository : IPhieuNhapKhoRepository
    {
        public readonly Dictionary<string, PhieuNhapKho> Data = new();

        public void LuuPhieuNhap(PhieuNhapKho phieu) => Data[phieu.MaPhieuNhap] = phieu;
        public PhieuNhapKho? TimTheoId(string maPhieu) => Data.TryGetValue(maPhieu, out var p) ? p : null;
        public List<PhieuNhapKho> LayTatCa() => Data.Values.OrderByDescending(p => p.NgayNhap).ToList();
        public void VoHieuHoaPhieuNhap(string maPhieuNhap)
        {
            if (Data.TryGetValue(maPhieuNhap, out var p))
            {
                p.TrangThai = "DaHuy";
            }
        }
        public void DuyetPhieuNhap(string maPhieuNhap)
        {
            if (Data.TryGetValue(maPhieuNhap, out var p))
            {
                p.TrangThai = "DaDuyet";
            }
        }
        public void XoaPhieuNhap(string maPhieuNhap)
        {
            Data.Remove(maPhieuNhap);
        }

        public Task LuuPhieuNhapAsync(PhieuNhapKho phieu, CancellationToken ct = default) { LuuPhieuNhap(phieu); return Task.CompletedTask; }
        public Task<PhieuNhapKho?> TimTheoIdAsync(string maPhieu, CancellationToken ct = default) => Task.FromResult(TimTheoId(maPhieu));
        public Task<List<PhieuNhapKho>> LayTatCaAsync(CancellationToken ct = default) => Task.FromResult(LayTatCa());
        public Task DuyetPhieuNhapAsync(string maPhieuNhap, CancellationToken ct = default) { DuyetPhieuNhap(maPhieuNhap); return Task.CompletedTask; }
        public Task XoaPhieuNhapAsync(string maPhieuNhap, CancellationToken ct = default) { XoaPhieuNhap(maPhieuNhap); return Task.CompletedTask; }
        public Task VoHieuHoaPhieuNhapAsync(string maPhieuNhap, CancellationToken ct = default) { VoHieuHoaPhieuNhap(maPhieuNhap); return Task.CompletedTask; }
    }

    [Fact]
    public void LapPhieuKiemKe_ThanhCong_TrangThaiChoDuyet_TonKhoChuaThayDoi()
    {
        var spRepo = new FakeSanPhamRepository();
        var kiemKeRepo = new FakePhieuKiemKeRepository();
        var useCase = new QuanLyKiemKeUseCase(kiemKeRepo, spRepo);

        var sp = new SanPhamTrangSuc("SP001", "Nhẫn Kim Cương", "DM01", "Vàng 18K", "Size 14", 10000000, 15000000, "SanSang", "", 10) { SoLuongTon = 10 };
        spRepo.LuuSanPham(sp);

        var dto = new TaoPhieuKiemKeDTO
        {
            MaPhieuKiemKe = "PKK001",
            ViTriKiemKe = "Kho Tổng",
            MaNhanVienKho = "NV_KHO",
            GhiChu = "Kiểm kê định kỳ tháng",
            DanhSachChiTiet = new List<ChiTietKiemKeItemDTO>
            {
                new() { MaSanPham = "SP001", TenSanPham = "Nhẫn Kim Cương", TonSoSach = 10, TonThucTe = 12 }
            }
        };

        var result = useCase.LapPhieuKiemKe(dto, out var error);

        Assert.True(result);
        Assert.Empty(error);

        var phieu = kiemKeRepo.TimTheoId("PKK001");
        Assert.NotNull(phieu);
        Assert.Equal("ChoDuyet", phieu.TrangThai);
        Assert.False(phieu.TrangThaiDuyet);

        // Tồn kho chưa bị thay đổi trước khi duyệt
        var spCheck = spRepo.TimTheoId("SP001");
        Assert.Equal(10, spCheck!.SoLuongTon);
    }

    [Fact]
    public void CapNhatPhieuKiemKe_ThanhCong_KhiOTrangThaiChoDuyet()
    {
        var spRepo = new FakeSanPhamRepository();
        var kiemKeRepo = new FakePhieuKiemKeRepository();
        var useCase = new QuanLyKiemKeUseCase(kiemKeRepo, spRepo);

        var phieu = new PhieuKiemKe("PKK002", "Kho Tổng", "NV_KHO", DateTime.UtcNow, "ChoDuyet", "Ghi chú cũ");
        phieu.ChiTietKiemKes.Add(new ChiTietKiemKe("PKK002", "SP001", 10, 8));
        kiemKeRepo.LuuPhieuKiemKe(phieu);

        var updateDto = new CapNhatPhieuKiemKeDTO
        {
            MaPhieuKiemKe = "PKK002",
            ViTriKiemKe = "Kho Quầy 1",
            GhiChu = "Đã cập nhật sau khi đếm lại",
            DanhSachChiTiet = new List<ChiTietKiemKeItemDTO>
            {
                new() { MaSanPham = "SP001", TonSoSach = 10, TonThucTe = 9, LyDoChenhLech = "Nhầm lẫn đợt 1" }
            }
        };

        var ok = useCase.CapNhatPhieuKiemKe(updateDto, out var err);

        Assert.True(ok);
        Assert.Empty(err);

        var phieuSau = kiemKeRepo.TimTheoId("PKK002");
        Assert.Equal("Đã cập nhật sau khi đếm lại", phieuSau!.GhiChu);
        Assert.Equal(9, phieuSau.ChiTietKiemKes.First().TonThucTe);
    }

    [Fact]
    public void XoaPhieuKiemKe_ThanhCong_KhiChoDuyet_VaChanKhiDaDuyet()
    {
        var spRepo = new FakeSanPhamRepository();
        var kiemKeRepo = new FakePhieuKiemKeRepository();
        var useCase = new QuanLyKiemKeUseCase(kiemKeRepo, spRepo);

        var phieu1 = new PhieuKiemKe("PKK_DEL_1", "Kho", "NV", DateTime.UtcNow, "ChoDuyet");
        var phieu2 = new PhieuKiemKe("PKK_DEL_2", "Kho", "NV", DateTime.UtcNow, "DaDuyet");
        kiemKeRepo.LuuPhieuKiemKe(phieu1);
        kiemKeRepo.LuuPhieuKiemKe(phieu2);

        // Xóa phiếu chưa duyệt -> OK
        var ok1 = useCase.XoaPhieuKiemKe("PKK_DEL_1", out var err1);
        Assert.True(ok1);
        Assert.Null(kiemKeRepo.TimTheoId("PKK_DEL_1"));

        // Xóa phiếu đã duyệt -> Chặn
        var ok2 = useCase.XoaPhieuKiemKe("PKK_DEL_2", out var err2);
        Assert.False(ok2);
        Assert.Contains("trạng thái Chờ duyệt", err2);
        Assert.NotNull(kiemKeRepo.TimTheoId("PKK_DEL_2"));
    }

    [Fact]
    public void DuyetPhieuKiemKe_ThanhCong_CapNhatTonKhoTheoDelta()
    {
        var spRepo = new FakeSanPhamRepository();
        var kiemKeRepo = new FakePhieuKiemKeRepository();
        var useCase = new QuanLyKiemKeUseCase(kiemKeRepo, spRepo);

        var sp = new SanPhamTrangSuc("SP001", "Dây Chuyền Ruby", "DM02", "Vàng 18K", "45cm", 5000000, 8000000, "SanSang", "", 10) { SoLuongTon = 10 };
        spRepo.LuuSanPham(sp);

        var phieu = new PhieuKiemKe("PKK_APPROVE", "Kho", "NV", DateTime.UtcNow, "ChoDuyet");
        phieu.ChiTietKiemKes.Add(new ChiTietKiemKe("PKK_APPROVE", "SP001", 10, 15)); // delta = +5
        kiemKeRepo.LuuPhieuKiemKe(phieu);

        var ok = useCase.DuyetPhieuKiemKe("PKK_APPROVE", "ADMIN", out var error);

        Assert.True(ok);
        Assert.Empty(error);

        var spCheck = spRepo.TimTheoId("SP001");
        Assert.Equal(15, spCheck!.SoLuongTon); // 10 + 5 = 15

        var phieuCheck = kiemKeRepo.TimTheoId("PKK_APPROVE");
        Assert.Equal("DaDuyet", phieuCheck!.TrangThai);
        Assert.True(phieuCheck.TrangThaiDuyet);
    }

    [Fact]
    public void DuyetPhieuKiemKe_ThanhCong_ChuyenTrangThaiHetHang_KhiTonThucTeBangKhong()
    {
        var spRepo = new FakeSanPhamRepository();
        var kiemKeRepo = new FakePhieuKiemKeRepository();
        var useCase = new QuanLyKiemKeUseCase(kiemKeRepo, spRepo);

        var sp = new SanPhamTrangSuc("SP008", "Nhẫn Bạc", "DM01", "Bạc 925", "Size 10", 1000000, 1500000, "SanSang", "", 0) { SoLuongTon = 3 };
        spRepo.LuuSanPham(sp);

        var phieu = new PhieuKiemKe("PKK_ZERO", "Kho", "NV", DateTime.UtcNow, "ChoDuyet");
        phieu.ChiTietKiemKes.Add(new ChiTietKiemKe("PKK_ZERO", "SP008", 3, 0)); // Kiểm kê thực tế mất hết hàng: tồn về 0
        kiemKeRepo.LuuPhieuKiemKe(phieu);

        var ok = useCase.DuyetPhieuKiemKe("PKK_ZERO", "ADMIN", out var error);

        Assert.True(ok);
        Assert.Empty(error);

        var spCheck = spRepo.TimTheoId("SP008");
        Assert.Equal(0, spCheck!.SoLuongTon);
        Assert.Equal("HetHang", spCheck.TrangThai);
    }

    [Fact]
    public void LapPhieuKiemKe_TuChoi_KhiTrungLapSanPhamTrongCungPhieu()
    {
        var spRepo = new FakeSanPhamRepository();
        var kiemKeRepo = new FakePhieuKiemKeRepository();
        var useCase = new QuanLyKiemKeUseCase(kiemKeRepo, spRepo);

        var sp = new SanPhamTrangSuc("SP001", "Nhẫn Kim Cương", "DM01", "Vàng 18K", "Size 14", 10000000, 15000000, "SanSang", "", 10) { SoLuongTon = 10 };
        spRepo.LuuSanPham(sp);

        var dto = new TaoPhieuKiemKeDTO
        {
            MaPhieuKiemKe = "PKK_DUP",
            ViTriKiemKe = "Kho Tổng",
            MaNhanVienKho = "NV_KHO",
            DanhSachChiTiet = new List<ChiTietKiemKeItemDTO>
            {
                new() { MaSanPham = "SP001", TonSoSach = 10, TonThucTe = 10 },
                new() { MaSanPham = "SP001", TonSoSach = 10, TonThucTe = 12 } // Trùng mã SP001
            }
        };

        var ok = useCase.LapPhieuKiemKe(dto, out var error);

        Assert.False(ok);
        Assert.Contains("bị trùng lặp trong biên bản kiểm kê", error);
        Assert.Null(kiemKeRepo.TimTheoId("PKK_DUP"));
    }

    [Fact]
    public void LapPhieuKiemKe_TuChoi_KhiTonSoSachKhongKhopHeThong()
    {
        var spRepo = new FakeSanPhamRepository();
        var kiemKeRepo = new FakePhieuKiemKeRepository();
        var useCase = new QuanLyKiemKeUseCase(kiemKeRepo, spRepo);

        // Tồn kho hệ thống thực tế đang là 10
        var sp = new SanPhamTrangSuc("SP001", "Nhẫn Kim Cương", "DM01", "Vàng 18K", "Size 14", 10000000, 15000000, "SanSang", "", 10) { SoLuongTon = 10 };
        spRepo.LuuSanPham(sp);

        // Gửi request với TonSoSach là 8 (lệch so với hệ thống 10)
        var dto = new TaoPhieuKiemKeDTO
        {
            MaPhieuKiemKe = "PKK_MISMATCH",
            ViTriKiemKe = "Kho Tổng",
            MaNhanVienKho = "NV_KHO",
            DanhSachChiTiet = new List<ChiTietKiemKeItemDTO>
            {
                new() { MaSanPham = "SP001", TonSoSach = 8, TonThucTe = 8 }
            }
        };

        var ok = useCase.LapPhieuKiemKe(dto, out var error);

        Assert.False(ok);
        Assert.Contains("không khớp với hệ thống tại thời điểm lập phiếu", error);
        Assert.Null(kiemKeRepo.TimTheoId("PKK_MISMATCH"));
    }

    [Fact]
    public void LapPhieuKiemKe_TuChoi_KhiTonThucTeAm()
    {
        var spRepo = new FakeSanPhamRepository();
        var kiemKeRepo = new FakePhieuKiemKeRepository();
        var useCase = new QuanLyKiemKeUseCase(kiemKeRepo, spRepo);

        var sp = new SanPhamTrangSuc("SP001", "Nhẫn Kim Cương", "DM01", "Vàng 18K", "Size 14", 10000000, 15000000, "SanSang", "", 10) { SoLuongTon = 10 };
        spRepo.LuuSanPham(sp);

        var dto = new TaoPhieuKiemKeDTO
        {
            MaPhieuKiemKe = "PKK_NEG",
            ViTriKiemKe = "Kho Tổng",
            MaNhanVienKho = "NV_KHO",
            DanhSachChiTiet = new List<ChiTietKiemKeItemDTO>
            {
                new() { MaSanPham = "SP001", TonSoSach = 10, TonThucTe = -2 }
            }
        };

        var ok = useCase.LapPhieuKiemKe(dto, out var error);

        Assert.False(ok);
        Assert.Contains("phải là số nguyên không âm", error);
    }

    [Fact]
    public void DuyetPhieuKiemKe_TuChoiDuyet_KhiTonKhoThucTeLechSoSachGhiTrongPhieu()
    {
        // Ràng buộc Anti-conflict: Khi duyệt, kiểm tra tồn kho hiện tại còn khớp TonSoSach không.
        // Giả sử sau khi lập phiếu, khách mua 2 cái nên tồn kho hệ thống giảm từ 10 xuống 8.
        var spRepo = new FakeSanPhamRepository();
        var kiemKeRepo = new FakePhieuKiemKeRepository();
        var useCase = new QuanLyKiemKeUseCase(kiemKeRepo, spRepo);

        var sp = new SanPhamTrangSuc("SP001", "Dây Chuyền Ruby", "DM02", "Vàng 18K", "45cm", 5000000, 8000000, "SanSang", "", 8) { SoLuongTon = 8 };
        spRepo.LuuSanPham(sp);

        var phieu = new PhieuKiemKe("PKK_CONFLICT", "Kho", "NV", DateTime.UtcNow, "ChoDuyet");
        phieu.ChiTietKiemKes.Add(new ChiTietKiemKe("PKK_CONFLICT", "SP001", 10, 12)); // Ghi nhận lúc tồn là 10
        kiemKeRepo.LuuPhieuKiemKe(phieu);

        var ok = useCase.DuyetPhieuKiemKe("PKK_CONFLICT", "ADMIN", out var error);

        Assert.False(ok);
        Assert.Contains("đã có sự biến động trên hệ thống", error);
        Assert.Contains("phát sinh giao dịch bán hàng hoặc nhập kho mới", error);

        // Tồn kho không bị thay đổi sai lệch
        var spCheck = spRepo.TimTheoId("SP001");
        Assert.Equal(8, spCheck!.SoLuongTon);
    }

    [Fact]
    public void VoHieuHoaPhieuNhap_ThanhCong_CapNhatTrangThaiVaTruTonKho()
    {
        var spRepo = new FakeSanPhamRepository();
        var nhapKhoRepo = new FakePhieuNhapKhoRepository();
        var useCase = new XacNhanNhapKhoUseCase(nhapKhoRepo, spRepo);

        var sp = new SanPhamTrangSuc("SP_INWARD", "Nhẫn Solitaire", "DM01", "Vàng 18K", "Size 14", 10000000, 16000000, "SanSang", "", 20) { SoLuongTon = 20 };
        spRepo.LuuSanPham(sp);

        var phieuNhap = new PhieuNhapKho("PNK_TEST", "NCC PNJ", "NV_ADMIN", DateTime.UtcNow, "DaDuyet")
        {
            TongSoSanPham = 5,
            TongGiaTri = 50000000
        };
        phieuNhap.ChiTietNhapKhos.Add(new ChiTietNhapKho("PNK_TEST", "SP_INWARD", 5, 10000000, "CT01"));
        nhapKhoRepo.LuuPhieuNhap(phieuNhap);

        var ok = useCase.VoHieuHoaPhieuNhap("PNK_TEST", "ADMIN", out var error);

        Assert.True(ok);
        Assert.Empty(error);

        // Phiếu nhập bị đánh dấu TrangThai = DaHuy
        var phieuCheck = nhapKhoRepo.TimTheoId("PNK_TEST");
        Assert.Equal("DaHuy", phieuCheck!.TrangThai);

        // Tồn kho bị trừ 5 món (20 - 5 = 15)
        var spCheck = spRepo.TimTheoId("SP_INWARD");
        Assert.Equal(15, spCheck!.SoLuongTon);
    }

    [Fact]
    public void DuyetPhieuNhap_CapNhatTonKho_GiaNhapBinhQuan_VaGiaBanTheoTyLeLoiNhuan()
    {
        var spRepo = new FakeSanPhamRepository();
        var nhapKhoRepo = new FakePhieuNhapKhoRepository();
        var useCase = new XacNhanNhapKhoUseCase(nhapKhoRepo, spRepo);

        // Ban đầu SP có giá nhập 10,000,000, tỷ lệ lợi nhuận 20%, tồn kho 0
        var sp = new SanPhamTrangSuc("SP_DUYET", "Nhẫn Ruby", "DM01", "Vàng 18K", "Size 14", 0, 0, "HetHang", "", 0)
        {
            TyLeLoiNhuan = 20
        };
        spRepo.LuuSanPham(sp);

        // Tạo phiếu nhập ở trạng thái ChoDuyet
        var request = new NhapKhoDTO
        {
            MaPhieuNhap = "PNK_DUYET_01",
            NhaCungCap = "NCC Ruby",
            MaNhanVien = "NV_KHO",
            DanhSachSanPham = new List<ChiTietNhapKhoDTO>
            {
                new() { MaSanPham = "SP_DUYET", SoLuong = 10, GiaVonNhap = 10000000 }
            }
        };
        useCase.Execute(request);

        // Phiếu vừa tạo phải ở trạng thái ChoDuyet, tồn kho vẫn là 0
        var phieuChuaDuyet = nhapKhoRepo.TimTheoId("PNK_DUYET_01");
        Assert.NotNull(phieuChuaDuyet);
        Assert.Equal("ChoDuyet", phieuChuaDuyet.TrangThai);
        Assert.Equal(0, spRepo.TimTheoId("SP_DUYET")!.SoLuongTon);

        // Admin duyệt phiếu
        var ok = useCase.DuyetPhieuNhap("PNK_DUYET_01", "ADMIN", out var error);
        Assert.True(ok);
        Assert.Empty(error);

        // Sau khi duyệt: phiếu thành DaDuyet, tồn kho = 10, giá vốn = 10,000,000, giá bán = 12,000,000 (lãi 20%)
        var phieuDaDuyet = nhapKhoRepo.TimTheoId("PNK_DUYET_01");
        Assert.Equal("DaDuyet", phieuDaDuyet!.TrangThai);

        var spCapNhat = spRepo.TimTheoId("SP_DUYET");
        Assert.Equal(10, spCapNhat!.SoLuongTon);
        Assert.Equal(10000000, spCapNhat.GiaNhap);
        Assert.Equal(12000000, spCapNhat.GiaBan);
    }

    [Fact]
    public void XoaPhieuNhap_ThanhCong_KhiPhieuChoDuyet()
    {
        var spRepo = new FakeSanPhamRepository();
        var nhapKhoRepo = new FakePhieuNhapKhoRepository();
        var useCase = new XacNhanNhapKhoUseCase(nhapKhoRepo, spRepo);

        var phieu = new PhieuNhapKho("PNK_XOA", "NCC Test", "NV01", DateTime.UtcNow, "ChoDuyet");
        nhapKhoRepo.LuuPhieuNhap(phieu);

        var ok = useCase.XoaPhieuNhap("PNK_XOA", out var error);
        Assert.True(ok);
        Assert.Empty(error);
        Assert.Null(nhapKhoRepo.TimTheoId("PNK_XOA"));
    }

    [Fact]
    public void VoHieuHoaPhieuNhap_ThatBai_KhiTonKhoTruDiBiAm()
    {
        var spRepo = new FakeSanPhamRepository();
        var nhapKhoRepo = new FakePhieuNhapKhoRepository();
        var useCase = new XacNhanNhapKhoUseCase(nhapKhoRepo, spRepo);

        // SP hiện tại chỉ còn tồn 2 cái (do đã bán mất 8 cái)
        var sp = new SanPhamTrangSuc("SP_AM", "Dây Chuyền", "DM01", "Bạc 925", "45cm", 2000000, 3000000, "SanSang", "", 0)
        {
            SoLuongTon = 2
        };
        spRepo.LuuSanPham(sp);

        // Phiếu nhập đã duyệt trước đó có số lượng 10 cái
        var phieu = new PhieuNhapKho("PNK_AM", "NCC Bạc", "ADMIN", DateTime.UtcNow, "DaDuyet");
        phieu.ChiTietNhapKhos.Add(new ChiTietNhapKho("PNK_AM", "SP_AM", 10, 2000000, "CT_AM"));
        nhapKhoRepo.LuuPhieuNhap(phieu);

        // Hủy phiếu nhập -> Tồn kho sau trừ là 2 - 10 = -8 (< 0) -> Phải từ chối!
        var ok = useCase.VoHieuHoaPhieuNhap("PNK_AM", "ADMIN", out var error);
        Assert.False(ok);
        Assert.Contains("âm", error, StringComparison.OrdinalIgnoreCase);

        // Tồn kho không bị thay đổi
        Assert.Equal(2, spRepo.TimTheoId("SP_AM")!.SoLuongTon);
    }

    [Fact]
    public void VoHieuHoaPhieuNhap_ThanhCong_KhoiPhucGiaVonVaGiaBan()
    {
        var spRepo = new FakeSanPhamRepository();
        var nhapKhoRepo = new FakePhieuNhapKhoRepository();
        var useCase = new XacNhanNhapKhoUseCase(nhapKhoRepo, spRepo);

        // Ban đầu SP có tồn kho 5 cái, giá vốn 1,000,000, giá bán 1,200,000 (lợi nhuận 20%)
        var sp = new SanPhamTrangSuc("SP_HOAN_NGUYEN", "Vòng Tay Bạc", "DM01", "Bạc 925", "16cm", 1000000m, 1200000m, "SanSang", "", 0, "0", 20m)
        {
            SoLuongTon = 5
        };
        spRepo.LuuSanPham(sp);

        // Nhập lô 2: 5 cái với giá vốn 2,000,000 (phiếu PNK_02)
        var phieu2 = new PhieuNhapKho("PNK_02", "NCC A", "ADMIN", DateTime.UtcNow, "DaDuyet");
        phieu2.ChiTietNhapKhos.Add(new ChiTietNhapKho("PNK_02", "SP_HOAN_NGUYEN", 5, 2000000m, "CT_02"));
        nhapKhoRepo.LuuPhieuNhap(phieu2);

        // Giả lập sau khi duyệt lô 2: Tồn kho = 10, Giá vốn bình quân = (5*1tr + 5*2tr) / 10 = 1,500,000. Giá bán = 1,800,000
        sp.SoLuongTon = 10;
        sp.GiaNhap = 1500000m;
        sp.GiaBan = 1800000m;

        // Thực hiện vô hiệu hóa phiếu PNK_02
        var ok = useCase.VoHieuHoaPhieuNhap("PNK_02", "ADMIN", out var error);

        // Kiểm tra: Hủy thành công, tồn kho còn lại là 5, giá vốn được hoàn nguyên về đúng 1,000,000 và giá bán về 1,200,000
        Assert.True(ok);
        Assert.Empty(error);
        var spSauHuy = spRepo.TimTheoId("SP_HOAN_NGUYEN");
        Assert.NotNull(spSauHuy);
        Assert.Equal(5, spSauHuy.SoLuongTon);
        Assert.Equal(1000000m, spSauHuy.GiaNhap);
        Assert.Equal(1200000m, spSauHuy.GiaBan);
    }

    [Fact]
    public void VoHieuHoaPhieuNhapDuyNhat_GiaVonVaGiaBanHoanNguyenVeKhong()
    {
        var spRepo = new FakeSanPhamRepository();
        var nhapKhoRepo = new FakePhieuNhapKhoRepository();
        var useCase = new XacNhanNhapKhoUseCase(nhapKhoRepo, spRepo);

        // Ban đầu SP mới tạo: Tồn kho = 0, Giá vốn = 0, Giá bán = 0, Tỷ lệ lãi = 20%
        var sp = new SanPhamTrangSuc("SP_DUY_NHAT", "Bông Tai Kim Cương", "DM03", "Bạch Kim", "Tiêu chuẩn", 0m, 0m, "HetHang", "", 0, "0", 20m)
        {
            SoLuongTon = 0
        };
        spRepo.LuuSanPham(sp);

        // Tạo và duyệt phiếu nhập duy nhất cho sản phẩm
        var phieu = new PhieuNhapKho("PNK_DUY_NHAT", "NCC Kim Cương", "ADMIN", DateTime.UtcNow, "DaDuyet");
        phieu.ChiTietNhapKhos.Add(new ChiTietNhapKho("PNK_DUY_NHAT", "SP_DUY_NHAT", 10, 5000000m, "CT_01"));
        nhapKhoRepo.LuuPhieuNhap(phieu);

        // Cập nhật trạng thái sau duyệt: Tồn 10, Giá vốn 5,000,000, Giá bán 6,000,000
        sp.SoLuongTon = 10;
        sp.GiaNhap = 5000000m;
        sp.GiaBan = 6000000m;
        sp.TrangThai = "SanSang";

        // Thực hiện vô hiệu hóa phiếu nhập duy nhất này
        var ok = useCase.VoHieuHoaPhieuNhap("PNK_DUY_NHAT", "ADMIN", out var error);

        // Kiểm tra: Hủy thành công, tồn kho về 0, giá vốn và giá bán hoàn nguyên về 0, trạng thái thành HetHang
        Assert.True(ok);
        Assert.Empty(error);
        var spSauHuy = spRepo.TimTheoId("SP_DUY_NHAT");
        Assert.NotNull(spSauHuy);
        Assert.Equal(0, spSauHuy.SoLuongTon);
        Assert.Equal(0m, spSauHuy.GiaNhap);
        Assert.Equal(0m, spSauHuy.GiaBan);
        Assert.Equal("HetHang", spSauHuy.TrangThai);
    }
}
