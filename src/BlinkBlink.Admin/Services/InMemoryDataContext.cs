using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlinkBlink.Domain.Entities.Cart;
using BlinkBlink.Domain.Entities.Catalog;
using BlinkBlink.Domain.Entities.Customers;
using BlinkBlink.Domain.Entities.Identity;
using BlinkBlink.Domain.Entities.Inventory;
using BlinkBlink.Domain.Entities.Orders;
using BlinkBlink.Domain.Entities.Warranty;
using BlinkBlink.Domain.Interfaces;

namespace BlinkBlink.Admin.Services;

/// <summary>
/// Kho dữ liệu mẫu lưu trữ trong bộ nhớ phục vụ hiển thị trực quan và kiểm thử trên WinForms Admin.
/// Tự động kích hoạt khi môi trường máy trạm chưa khởi chạy máy chủ SQL Server.
/// </summary>
public class InMemoryDataContext
{
    public List<SanPhamTrangSuc> SanPhams { get; } = new();
    public List<NhomSanPham> NhomSanPhams { get; } = new();
    public List<DanhMuc> DanhMucs { get; } = new();
    public List<HoaDon> HoaDons { get; } = new();
    public List<PhieuBaoHanh> PhieuBaoHanhs { get; } = new();
    public List<NhanVien> NhanViens { get; } = new();
    public List<KhachHang> KhachHangs { get; } = new();
    public List<VaiTro> VaiTros { get; } = new();
    public List<PhieuNhapKho> PhieuNhapKhos { get; } = new();
    public List<PhieuKiemKe> PhieuKiemKes { get; } = new();

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

    public InMemoryDataContext()
    {
        SanPhamRepo = new MemorySanPhamRepository(this);
        NhomSanPhamRepo = new MemoryNhomSanPhamRepository(this);
        DanhMucRepo = new MemoryDanhMucRepository(this);
        HoaDonRepo = new MemoryHoaDonRepository(this);
        GioHangRepo = new MemoryGioHangRepository();
        BaoHanhRepo = new MemoryBaoHanhRepository(this);
        NhanVienRepo = new MemoryNhanVienRepository(this);
        KhachHangRepo = new MemoryKhachHangRepository(this);
        VaiTroRepo = new MemoryVaiTroRepository(this);
        ThongKeRepo = new MemoryThongKeRepository(this);
        PhieuNhapKhoRepo = new MemoryPhieuNhapKhoRepository(this);
        PhieuKiemKeRepo = new MemoryPhieuKiemKeRepository(this);
        NguoiDungRepo = new MemoryNguoiDungRepository(this);
    }

    public void SeedInitialLuxuryData()
    {
        // 1. Vai trò hệ thống
        VaiTros.Clear();
        VaiTros.Add(new VaiTro("ADMIN", "Quản trị viên", "Quản trị toàn quyền hệ thống cửa hàng kim hoàn"));
        VaiTros.Add(new VaiTro("SALE", "Nhân viên bán hàng", "Thu ngân và tư vấn bán hàng tại quầy POS"));
        VaiTros.Add(new VaiTro("WAREHOUSE", "Thủ kho", "Quản lý nhập kho và lập biên bản kiểm kê trang sức"));
        VaiTros.Add(new VaiTro("TECHNICIAN", "Nhân viên kỹ thuật", "Bảo dưỡng, sửa chữa và quyết toán chi phí bảo hành"));
        VaiTros.Add(new VaiTro("KHACHHANG", "Khách hàng", "Khách hàng mua sắm trực tuyến và tra cứu bảo hành"));

        // 2. Tài khoản nhân sự mặc định (Đảm bảo luôn đăng nhập được với admin / 123456789)
        NhanViens.Clear();
        NhanViens.Add(new NhanVien("USER_ADMIN01", "Trần Minh Hoàng", "0901234567", "admin", "123456789", "ADMIN", "NV_ADMIN", 25000000, true));
        NhanViens.Add(new NhanVien("USER_NV001", "Lê Phương Thảo", "0912345678", "thao.le", "123456789", "SALE", "NV001", 12000000, true));
        NhanViens.Add(new NhanVien("USER_NV002", "Nguyễn Quốc Huy", "0933445566", "huy.nguyen", "123456789", "WAREHOUSE", "NV002", 14000000, true));
        NhanViens.Add(new NhanVien("USER_NV003", "Trần Kỹ Thuật", "0944556677", "kythuat", "123456789", "TECHNICIAN", "NV003", 13500000, true));

        // 3. Danh mục sản phẩm kim hoàn
        DanhMucs.Clear();
        DanhMucs.Add(new DanhMuc("DM01", "Nhẫn Kim Cương", "Bộ sưu tập nhẫn đính hôn và nhẫn cưới"));
        DanhMucs.Add(new DanhMuc("DM02", "Dây Chuyền & Mặt Dây", "Thiết kế vàng 18K và đá quý thiên nhiên"));
        DanhMucs.Add(new DanhMuc("DM03", "Bông Tai Quý Phái", "Bông tai bạch kim và sapphire"));
        DanhMucs.Add(new DanhMuc("DM04", "Lắc Tay & Vòng Tay", "Vòng tay tennis kim cương cao cấp"));

        // 3.1. Nhóm sản phẩm (gom nhóm các sản phẩm cùng mẫu khác size)
        NhomSanPhams.Clear();
        NhomSanPhams.Add(new NhomSanPham("NHOM01", "Nhẫn Solitaire Diamond 18K", "Nhẫn kim cương Solitaire các cỡ"));

        // 4. Sản phẩm trang sức mẫu
        SanPhams.Clear();
        SanPhams.Add(new SanPhamTrangSuc("SP001", "Nhẫn Solitaire Diamond 18K", "DM01", "Vàng trắng 18K đính kim cương chủ 5.4 ly", "Size 14", 18000000, 28500000, "SanSang", "https://images.unsplash.com/photo-1605100804763-247f67b3557e?auto=format&fit=crop&w=800&q=80", 10, "NHOM01") { SoLuongTon = 10 });
        SanPhams.Add(new SanPhamTrangSuc("SP001_15", "Nhẫn Solitaire Diamond 18K", "DM01", "Vàng trắng 18K đính kim cương chủ 5.4 ly", "Size 15", 18000000, 28500000, "SanSang", "https://images.unsplash.com/photo-1605100804763-247f67b3557e?auto=format&fit=crop&w=800&q=80", 10, "NHOM01") { SoLuongTon = 7 });
        SanPhams.Add(new SanPhamTrangSuc("SP002", "Dây Chuyền Eternal Heart Ruby", "DM02", "Vàng hồng 18K đính đá Ruby huyết bồ câu", "Size 45cm", 12000000, 19200000, "SanSang", "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?auto=format&fit=crop&w=800&q=80", 0, "0") { SoLuongTon = 8 });
        SanPhams.Add(new SanPhamTrangSuc("SP003", "Bông Tai Royal Blue Sapphire", "DM03", "Bạch kim Platin 950 đính Sapphire Hoàng gia", "Tiêu chuẩn", 22000000, 34000000, "SanSang", "https://images.unsplash.com/photo-1630019852942-f89202989a59?auto=format&fit=crop&w=800&q=80", 15) { SoLuongTon = 5 });
        SanPhams.Add(new SanPhamTrangSuc("SP004", "Lắc Tay Tennis Diamond Bracelet", "DM04", "Vàng trắng 18K đính 45 viên kim cương tự nhiên", "Size 17cm", 48000000, 72000000, "SanSang", "https://images.unsplash.com/photo-1611591475152-47354900a359?auto=format&fit=crop&w=800&q=80", 20) { SoLuongTon = 4 });
        SanPhams.Add(new SanPhamTrangSuc("SP005", "Nhẫn Cưới Harmony Eternity", "DM01", "Vàng Ý 750 tinh tế đính kim cương tấm viền", "Size 16", 11000000, 16800000, "SanSang", "https://images.unsplash.com/photo-1603561591411-07134e71a2a9?auto=format&fit=crop&w=800&q=80", 5) { SoLuongTon = 12 });
        SanPhams.Add(new SanPhamTrangSuc("SP006", "Mặt Dây Chuyền Ngọc Trai South Sea", "DM02", "Vàng vàng 18K đính ngọc trai biển và kim cương", "Size 42cm", 28000000, 42500000, "SanSang", "https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?auto=format&fit=crop&w=800&q=80", 0) { SoLuongTon = 6 });

        // 5. Phiếu bảo hành mẫu
        PhieuBaoHanhs.Clear();
        PhieuBaoHanhs.Add(new PhieuBaoHanh(
            soPhieuBH: "PBH_DEMO01",
            maKhachHang: "KH001",
            maSanPham: "SP001",
            lyDoBaoHanh: "Mặt nhẫn trầy nhẹ, cần đánh bóng",
            chiPhiUocTinh: 0,
            trangThaiXuLy: "TiepNhan",
            ngayTiepNhan: DateTime.Now.AddDays(-2),
            ngayHenTra: DateTime.Now.AddDays(2),
            chiPhiThucTe: 0,
            tinhTrangTiepNhan: "Chấu hơi lỏng",
            ghiChu: "Khách cần gấp trước cuối tuần"
        ));
        PhieuBaoHanhs.Add(new PhieuBaoHanh(
            soPhieuBH: "PBH_DEMO02",
            maKhachHang: "KH002",
            maSanPham: "SP003",
            lyDoBaoHanh: "Rơi viên đá tấm 1.5 ly",
            chiPhiUocTinh: 150000,
            trangThaiXuLy: "DangXuLy",
            ngayTiepNhan: DateTime.Now.AddDays(-3),
            ngayHenTra: DateTime.Now.AddDays(1),
            chiPhiThucTe: 0,
            tinhTrangTiepNhan: "Mất 1 viên tấm bên trái",
            ghiChu: "Kỹ thuật đang đính lại chấu"
        ));
        PhieuBaoHanhs.Add(new PhieuBaoHanh(
            soPhieuBH: "PBH_DEMO03",
            maKhachHang: "KH001",
            maSanPham: "SP004",
            lyDoBaoHanh: "Xi mạ làm mới bạch kim",
            chiPhiUocTinh: 200000,
            trangThaiXuLy: "DaXuLy",
            ngayTiepNhan: DateTime.Now.AddDays(-5),
            ngayHenTra: DateTime.Now.AddDays(-1),
            chiPhiThucTe: 250000,
            tinhTrangTiepNhan: "Trầy xước do va quẹt",
            ghiChu: "Đã xi mạ xong, chờ khách đến nhận"
        ));
    }

    private class MemorySanPhamRepository : ISanPhamRepository
    {
        private readonly InMemoryDataContext _ctx;
        public MemorySanPhamRepository(InMemoryDataContext ctx) => _ctx = ctx;

        public void LuuSanPham(SanPhamTrangSuc sp)
        {
            var existing = _ctx.SanPhams.FirstOrDefault(s => s.MaSanPham == sp.MaSanPham);
            if (existing != null) _ctx.SanPhams.Remove(existing);
            _ctx.SanPhams.Add(sp);
        }
        public SanPhamTrangSuc? TimTheoId(string id) => _ctx.SanPhams.FirstOrDefault(s => s.MaSanPham == id);
        public List<SanPhamTrangSuc> LayTatCa() => _ctx.SanPhams.ToList();
        public List<SanPhamTrangSuc> LayTheoNhom(string maNhom) => _ctx.SanPhams.Where(s => s.MaNhom == maNhom).ToList();
        public Task LuuSanPhamAsync(SanPhamTrangSuc sp, CancellationToken ct = default) { LuuSanPham(sp); return Task.CompletedTask; }
        public Task<SanPhamTrangSuc?> TimTheoIdAsync(string id, CancellationToken ct = default) => Task.FromResult(TimTheoId(id));
        public Task<List<SanPhamTrangSuc>> LayTatCaAsync(CancellationToken ct = default) => Task.FromResult(LayTatCa());
        public Task<List<SanPhamTrangSuc>> LayTheoNhomAsync(string maNhom, CancellationToken ct = default) => Task.FromResult(LayTheoNhom(maNhom));
    }

    private class MemoryNhomSanPhamRepository : INhomSanPhamRepository
    {
        private readonly InMemoryDataContext _ctx;
        public MemoryNhomSanPhamRepository(InMemoryDataContext ctx) => _ctx = ctx;

        public void LuuNhom(NhomSanPham nhom)
        {
            var existing = _ctx.NhomSanPhams.FirstOrDefault(n => n.MaNhom == nhom.MaNhom);
            if (existing != null) _ctx.NhomSanPhams.Remove(existing);
            _ctx.NhomSanPhams.Add(nhom);
        }
        public void CapNhatNhom(NhomSanPham nhom) => LuuNhom(nhom);
        public bool XoaNhom(string maNhom)
        {
            var existing = _ctx.NhomSanPhams.FirstOrDefault(n => n.MaNhom == maNhom);
            if (existing == null) return false;
            return _ctx.NhomSanPhams.Remove(existing);
        }
        public NhomSanPham? TimTheoId(string maNhom) => _ctx.NhomSanPhams.FirstOrDefault(n => n.MaNhom == maNhom);
        public List<NhomSanPham> LayTatCa() => _ctx.NhomSanPhams.ToList();

        public Task LuuNhomAsync(NhomSanPham nhom, CancellationToken ct = default) { LuuNhom(nhom); return Task.CompletedTask; }
        public Task CapNhatNhomAsync(NhomSanPham nhom, CancellationToken ct = default) { CapNhatNhom(nhom); return Task.CompletedTask; }
        public Task<bool> XoaNhomAsync(string maNhom, CancellationToken ct = default) => Task.FromResult(XoaNhom(maNhom));
        public Task<NhomSanPham?> TimTheoIdAsync(string maNhom, CancellationToken ct = default) => Task.FromResult(TimTheoId(maNhom));
        public Task<List<NhomSanPham>> LayTatCaAsync(CancellationToken ct = default) => Task.FromResult(LayTatCa());
    }

    private class MemoryDanhMucRepository : IDanhMucRepository
    {
        private readonly InMemoryDataContext _ctx;
        public MemoryDanhMucRepository(InMemoryDataContext ctx) => _ctx = ctx;
        public void LuuDanhMuc(DanhMuc dm) { _ctx.DanhMucs.Add(dm); }
        public Task LuuDanhMucAsync(DanhMuc dm, CancellationToken ct = default) { LuuDanhMuc(dm); return Task.CompletedTask; }
        public DanhMuc? TimTheoId(string id) => _ctx.DanhMucs.FirstOrDefault(d => d.MaDanhMuc == id);
        public Task<DanhMuc?> TimTheoIdAsync(string id, CancellationToken ct = default) => Task.FromResult(TimTheoId(id));
        public List<DanhMuc> LayTatCa() => _ctx.DanhMucs.ToList();
        public Task<List<DanhMuc>> LayTatCaAsync(CancellationToken ct = default) => Task.FromResult(LayTatCa());
    }

    private class MemoryHoaDonRepository : IHoaDonRepository
    {
        private readonly InMemoryDataContext _ctx;
        public MemoryHoaDonRepository(InMemoryDataContext ctx) => _ctx = ctx;

        public void LuuHoaDon(HoaDon hd) => _ctx.HoaDons.Add(hd);
        public Task LuuHoaDonAsync(HoaDon hd, CancellationToken ct = default) { LuuHoaDon(hd); return Task.CompletedTask; }
        public HoaDon? TimTheoMa(string ma) => _ctx.HoaDons.FirstOrDefault(h => h.MaHoaDon == ma);
        public Task<HoaDon?> TimTheoMaAsync(string ma, CancellationToken ct = default) => Task.FromResult(TimTheoMa(ma));
        public List<HoaDon> LayTheoKhachHang(string maKH) => _ctx.HoaDons.Where(h => h.MaKhachHang == maKH).ToList();
        public Task<List<HoaDon>> LayTheoKhachHangAsync(string maKH, CancellationToken ct = default) => Task.FromResult(LayTheoKhachHang(maKH));
        public List<HoaDon> LayDanhSachDonHang() => _ctx.HoaDons.OrderByDescending(h => h.NgayLap).ToList();
        public Task<List<HoaDon>> LayDanhSachDonHangAsync(CancellationToken ct = default) => Task.FromResult(LayDanhSachDonHang());
        public void CapNhatHoaDon(HoaDon hd)
        {
            var idx = _ctx.HoaDons.FindIndex(h => h.MaHoaDon == hd.MaHoaDon);
            if (idx >= 0) _ctx.HoaDons[idx] = hd;
        }
        public Task CapNhatHoaDonAsync(HoaDon hd, CancellationToken ct = default) { CapNhatHoaDon(hd); return Task.CompletedTask; }

        public void CapNhatKhachHangTheoSoDienThoai(string soDienThoai, string maKhachHangMoi)
        {
            if (string.IsNullOrWhiteSpace(soDienThoai) || string.IsNullOrWhiteSpace(maKhachHangMoi)) return;
            foreach (var hd in _ctx.HoaDons.Where(h => h.SoDienThoaiNhan == soDienThoai && (h.MaKhachHang == "0" || h.MaKhachHang == "KH_0")))
            {
                hd.MaKhachHang = maKhachHangMoi;
            }
        }
    }

    private class MemoryGioHangRepository : IGioHangRepository
    {
        private readonly Dictionary<string, GioHang> _carts = new();
        public GioHang? LayGioHang(string maKH) => _carts.TryGetValue(maKH, out var gh) ? gh : null;
        public Task<GioHang?> LayGioHangAsync(string maKH, CancellationToken ct = default) => Task.FromResult(LayGioHang(maKH));
        public void CapNhatGioHang(GioHang gh) => _carts[gh.MaKhachHang] = gh;
        public Task CapNhatGioHangAsync(GioHang gh, CancellationToken ct = default) { CapNhatGioHang(gh); return Task.CompletedTask; }
        public Task LuuGioHangAsync(GioHang gh, CancellationToken ct = default) { CapNhatGioHang(gh); return Task.CompletedTask; }
    }

    private class MemoryBaoHanhRepository : IBaoHanhRepository
    {
        private readonly InMemoryDataContext _ctx;
        public MemoryBaoHanhRepository(InMemoryDataContext ctx) => _ctx = ctx;

        public void LuuPhieu(PhieuBaoHanh pbh) => _ctx.PhieuBaoHanhs.Add(pbh);
        public Task LuuPhieuAsync(PhieuBaoHanh pbh, CancellationToken ct = default) { LuuPhieu(pbh); return Task.CompletedTask; }
        public PhieuBaoHanh? TimTheoSoPhieu(string soPhieu) => _ctx.PhieuBaoHanhs.FirstOrDefault(p => p.SoPhieuBH == soPhieu);
        public Task<PhieuBaoHanh?> TimTheoSoPhieuAsync(string soPhieu, CancellationToken ct = default) => Task.FromResult(TimTheoSoPhieu(soPhieu));
        public List<PhieuBaoHanh> LayTheoKhachHang(string maKH) => _ctx.PhieuBaoHanhs.Where(p => p.MaKhachHang == maKH).ToList();
        public Task<List<PhieuBaoHanh>> LayTheoKhachHangAsync(string maKH, CancellationToken ct = default) => Task.FromResult(LayTheoKhachHang(maKH));
        public List<PhieuBaoHanh> LayDanhSachPhieu() => _ctx.PhieuBaoHanhs.OrderByDescending(p => p.NgayTiepNhan).ToList();
        public Task<List<PhieuBaoHanh>> LayDanhSachPhieuAsync(CancellationToken ct = default) => Task.FromResult(LayDanhSachPhieu());
        public void CapNhatPhieu(PhieuBaoHanh pbh)
        {
            var idx = _ctx.PhieuBaoHanhs.FindIndex(p => p.SoPhieuBH == pbh.SoPhieuBH);
            if (idx >= 0) _ctx.PhieuBaoHanhs[idx] = pbh;
        }
        public Task CapNhatPhieuAsync(PhieuBaoHanh pbh, CancellationToken ct = default) { CapNhatPhieu(pbh); return Task.CompletedTask; }

        public void CapNhatKhachHangTheoSoDienThoai(string soDienThoai, string maKhachHangMoi)
        {
            if (string.IsNullOrWhiteSpace(soDienThoai) || string.IsNullOrWhiteSpace(maKhachHangMoi)) return;
            var sdt = soDienThoai.Trim();
            var matched = _ctx.PhieuBaoHanhs
                .Where(p => !string.IsNullOrEmpty(p.SoDienThoai) &&
                            string.Equals(p.SoDienThoai.Trim(), sdt, StringComparison.OrdinalIgnoreCase) &&
                            (p.MaKhachHang == "0" || p.MaKhachHang == "KH_0" || p.MaKhachHang == "KH_GUEST_POS"))
                .ToList();
            foreach (var pbh in matched)
            {
                pbh.MaKhachHang = maKhachHangMoi;
            }
        }
    }

    private class MemoryNhanVienRepository : INhanVienRepository
    {
        private readonly InMemoryDataContext _ctx;
        public MemoryNhanVienRepository(InMemoryDataContext ctx) => _ctx = ctx;
        public void LuuNhanVien(NhanVien nv)
        {
            var existing = _ctx.NhanViens.FirstOrDefault(n => n.MaNhanVien == nv.MaNhanVien || n.MaNguoiDung == nv.MaNguoiDung);
            if (existing != null) _ctx.NhanViens.Remove(existing);
            _ctx.NhanViens.Add(nv);
        }
        public void CapNhatNhanVien(NhanVien nv) => LuuNhanVien(nv);
        public NhanVien? TimTheoMa(string maNV) => _ctx.NhanViens.FirstOrDefault(n => n.MaNhanVien == maNV || n.MaNguoiDung == maNV);
        public List<NhanVien> LayTatCa() => _ctx.NhanViens.ToList();
        public bool KiemTraCoTheXoa(string maNV)
        {
            var hasNhapKho = _ctx.PhieuNhapKhos.Any(p => p.MaNhanVien == maNV);
            var hasHoaDon = _ctx.HoaDons.Any(h => h.MaNhanVien == maNV);
            var hasKiemKe = _ctx.PhieuKiemKes.Any(k => k.MaNhanVienKho == maNV);
            return !hasNhapKho && !hasHoaDon && !hasKiemKe;
        }
        public bool XoaNhanVien(string maNV)
        {
            if (!KiemTraCoTheXoa(maNV)) return false;
            var item = TimTheoMa(maNV);
            if (item != null) return _ctx.NhanViens.Remove(item);
            return false;
        }
    }

    private class MemoryKhachHangRepository : IKhachHangRepository
    {
        private readonly InMemoryDataContext _ctx;
        public MemoryKhachHangRepository(InMemoryDataContext ctx) => _ctx = ctx;
        public void LuuKhachHang(KhachHang kh)
        {
            var existing = _ctx.KhachHangs.FirstOrDefault(k => k.MaKhachHang == kh.MaKhachHang || k.MaNguoiDung == kh.MaNguoiDung);
            if (existing != null) _ctx.KhachHangs.Remove(existing);
            _ctx.KhachHangs.Add(kh);
        }
        public KhachHang? TimTheoMa(string maKH) => _ctx.KhachHangs.FirstOrDefault(k => k.MaKhachHang == maKH || k.MaNguoiDung == maKH);
        public KhachHang? TimTheoSoDienThoai(string soDienThoai) => _ctx.KhachHangs.FirstOrDefault(k => k.SoDienThoai == soDienThoai);
        public List<KhachHang> LayTatCa() => _ctx.KhachHangs.ToList();
        public bool KiemTraCoTheXoa(string maKH)
        {
            var hasDon = _ctx.HoaDons.Any(h => h.MaKhachHang == maKH);
            var hasBaoHanh = _ctx.PhieuBaoHanhs.Any(b => b.MaKhachHang == maKH);
            return !hasDon && !hasBaoHanh;
        }
        public bool XoaKhachHang(string maKH)
        {
            if (!KiemTraCoTheXoa(maKH)) return false;
            var item = TimTheoMa(maKH);
            if (item != null) return _ctx.KhachHangs.Remove(item);
            return false;
        }
    }

    private class MemoryVaiTroRepository : IVaiTroRepository
    {
        private readonly InMemoryDataContext _ctx;
        public MemoryVaiTroRepository(InMemoryDataContext ctx) => _ctx = ctx;
        public VaiTro? TimTheoMa(string maVaiTro) => _ctx.VaiTros.FirstOrDefault(v => v.MaVaiTro == maVaiTro);
        public List<VaiTro> LayTatCa() => _ctx.VaiTros.ToList();
        public void LuuVaiTro(VaiTro vt) => _ctx.VaiTros.Add(vt);
    }

    private class MemoryThongKeRepository : IThongKeRepository
    {
        private readonly InMemoryDataContext _ctx;
        public MemoryThongKeRepository(InMemoryDataContext ctx) => _ctx = ctx;

        public List<HoaDon> TruyVanDoanhThu(DateTime tuNgay, DateTime denNgay)
        {
            return _ctx.HoaDons
                .Where(h => h.NgayLap.Date >= tuNgay.Date && h.NgayLap.Date <= denNgay.Date && h.TrangThaiDonHang != "DaHuy")
                .ToList();
        }
        public Task<List<HoaDon>> TruyVanDoanhThuAsync(DateTime tu, DateTime den, CancellationToken ct = default) => Task.FromResult(TruyVanDoanhThu(tu, den));

        public List<SanPhamTrangSuc> TruyVanSanPhamTonKho() => _ctx.SanPhams.ToList();
        public Task<List<SanPhamTrangSuc>> TruyVanSanPhamTonKhoAsync(CancellationToken ct = default) => Task.FromResult(TruyVanSanPhamTonKho());

        public List<ChiTietKiemKe> TruyVanChenhLechKiemKe() => new();
        public Task<List<ChiTietKiemKe>> TruyVanChenhLechKiemKeAsync(CancellationToken ct = default) => Task.FromResult(TruyVanChenhLechKiemKe());

        public List<SanPhamTrangSuc> TruyVanSanPhamDoanhThuThap() => _ctx.SanPhams.Take(2).ToList();
        public Task<List<SanPhamTrangSuc>> TruyVanSanPhamDoanhThuThapAsync(CancellationToken ct = default) => Task.FromResult(TruyVanSanPhamDoanhThuThap());
    }

    private class MemoryPhieuNhapKhoRepository : IPhieuNhapKhoRepository
    {
        private readonly InMemoryDataContext _ctx;
        public MemoryPhieuNhapKhoRepository(InMemoryDataContext ctx) => _ctx = ctx;
        public void LuuPhieuNhap(PhieuNhapKho phieu)
        {
            var existing = _ctx.PhieuNhapKhos.FirstOrDefault(p => p.MaPhieuNhap == phieu.MaPhieuNhap);
            if (existing != null) _ctx.PhieuNhapKhos.Remove(existing);
            _ctx.PhieuNhapKhos.Add(phieu);
        }
        public Task LuuPhieuNhapAsync(PhieuNhapKho phieu, CancellationToken ct = default) { LuuPhieuNhap(phieu); return Task.CompletedTask; }
        public PhieuNhapKho? TimTheoId(string id) => _ctx.PhieuNhapKhos.FirstOrDefault(p => p.MaPhieuNhap == id);
        public Task<PhieuNhapKho?> TimTheoIdAsync(string id, CancellationToken ct = default) => Task.FromResult(TimTheoId(id));
        public List<PhieuNhapKho> LayTatCa() => _ctx.PhieuNhapKhos.OrderByDescending(p => p.NgayNhap).ToList();
        public Task<List<PhieuNhapKho>> LayTatCaAsync(CancellationToken ct = default) => Task.FromResult(LayTatCa());
        public void VoHieuHoaPhieuNhap(string maPhieuNhap)
        {
            var p = TimTheoId(maPhieuNhap);
            if (p != null) p.TrangThai = "DaHuy";
        }
        public void DuyetPhieuNhap(string maPhieuNhap)
        {
            var p = TimTheoId(maPhieuNhap);
            if (p != null) p.TrangThai = "DaDuyet";
        }
        public void XoaPhieuNhap(string maPhieuNhap)
        {
            var p = TimTheoId(maPhieuNhap);
            if (p != null) _ctx.PhieuNhapKhos.Remove(p);
        }
        public Task DuyetPhieuNhapAsync(string maPhieuNhap, CancellationToken ct = default) { DuyetPhieuNhap(maPhieuNhap); return Task.CompletedTask; }
        public Task XoaPhieuNhapAsync(string maPhieuNhap, CancellationToken ct = default) { XoaPhieuNhap(maPhieuNhap); return Task.CompletedTask; }
    }

    private class MemoryPhieuKiemKeRepository : IPhieuKiemKeRepository
    {
        private readonly InMemoryDataContext _ctx;
        public MemoryPhieuKiemKeRepository(InMemoryDataContext ctx) => _ctx = ctx;
        public void LuuPhieuKiemKe(PhieuKiemKe phieu)
        {
            var existing = _ctx.PhieuKiemKes.FirstOrDefault(p => p.MaPhieuKiemKe == phieu.MaPhieuKiemKe);
            if (existing != null) _ctx.PhieuKiemKes.Remove(existing);
            _ctx.PhieuKiemKes.Add(phieu);
        }
        public Task LuuPhieuKiemKeAsync(PhieuKiemKe phieu, CancellationToken ct = default) { LuuPhieuKiemKe(phieu); return Task.CompletedTask; }
        public PhieuKiemKe? TimTheoId(string id) => _ctx.PhieuKiemKes.FirstOrDefault(p => p.MaPhieuKiemKe == id);
        public Task<PhieuKiemKe?> TimTheoIdAsync(string id, CancellationToken ct = default) => Task.FromResult(TimTheoId(id));
        public List<PhieuKiemKe> LayTatCa() => _ctx.PhieuKiemKes.OrderByDescending(k => k.NgayKiemKe).ToList();
        public void XoaPhieuKiemKe(string maPhieu)
        {
            var item = TimTheoId(maPhieu);
            if (item != null && string.Equals(item.TrangThai, "ChoDuyet", StringComparison.OrdinalIgnoreCase))
            {
                _ctx.PhieuKiemKes.Remove(item);
            }
        }
        public void CapNhatTrangThai(string maPhieu, string trangThaiMoi, DateTime? ngayDuyet = null)
        {
            var item = TimTheoId(maPhieu);
            if (item != null)
            {
                item.TrangThai = trangThaiMoi;
                item.TrangThaiDuyet = string.Equals(trangThaiMoi, "DaDuyet", StringComparison.OrdinalIgnoreCase);
                if (ngayDuyet.HasValue) item.NgayDuyet = ngayDuyet.Value;
            }
        }
        public bool KiemTraCoPhieuKiemKeMoiHon(string maPhieu, string maSanPham)
        {
            var target = TimTheoId(maPhieu);
            if (target == null) return false;
            var targetDate = target.NgayDuyet ?? target.NgayKiemKe;
            return _ctx.PhieuKiemKes.Any(k => k.MaPhieuKiemKe != maPhieu
                && string.Equals(k.TrangThai, "DaDuyet", StringComparison.OrdinalIgnoreCase)
                && (k.NgayDuyet ?? k.NgayKiemKe) > targetDate
                && k.ChiTietKiemKes.Any(ct => ct.MaSanPham == maSanPham));
        }
    }

    private class MemoryNguoiDungRepository : INguoiDungRepository
    {
        private readonly InMemoryDataContext _ctx;
        public MemoryNguoiDungRepository(InMemoryDataContext ctx) => _ctx = ctx;
        public NguoiDung? TimTheoTenDangNhap(string tenDN)
        {
            var nv = _ctx.NhanViens.FirstOrDefault(n => string.Equals(n.TenDangNhap, tenDN, StringComparison.OrdinalIgnoreCase));
            if (nv != null) return nv;
            return _ctx.KhachHangs.FirstOrDefault(k => string.Equals(k.TenDangNhap, tenDN, StringComparison.OrdinalIgnoreCase));
        }
        public void CapNhat(NguoiDung nd)
        {
            if (nd is NhanVien nv)
            {
                var idx = _ctx.NhanViens.FindIndex(n => n.MaNguoiDung == nd.MaNguoiDung);
                if (idx >= 0) _ctx.NhanViens[idx] = nv;
            }
            else if (nd is KhachHang kh)
            {
                var idx = _ctx.KhachHangs.FindIndex(k => k.MaNguoiDung == nd.MaNguoiDung);
                if (idx >= 0) _ctx.KhachHangs[idx] = kh;
            }
        }
    }
}
