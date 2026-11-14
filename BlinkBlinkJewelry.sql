-- ====================================================================================
-- DỰ ÁN BLINK BLINK JEWELRY - HỆ THỐNG QUẢN LÝ VÀ BÁN TRANG SỨC CAO CẤP
-- KỊCH BẢN KHỞI TẠO CƠ SỞ DỮ LIỆU CHUẨN SQL SERVER (DDL & DML)
-- Kiến trúc: Clean Architecture - Truy xuất dữ liệu: ADO.NET thuần
-- Đối chiếu: Khớp 100% với Class Diagram kỹ thuật
-- ====================================================================================

USE master;
GO

-- 1. TẠO CƠ SỞ DỮ LIỆU NẾU CHƯA CÓ
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BlinkBlinkJewelryDb')
BEGIN
    CREATE DATABASE BlinkBlinkJewelryDb;
END
GO

USE BlinkBlinkJewelryDb;
GO

-- ====================================================================================
-- PHẦN I: XÓA CÁC BẢNG THEO THỨ TỰ RÀNG BUỘC KHÓA NGOẠI NẾU TỒN TẠI (CLEAN SLATE)
-- ====================================================================================
IF OBJECT_ID('dbo.PhieuBaoHanh', 'U') IS NOT NULL DROP TABLE dbo.PhieuBaoHanh;
IF OBJECT_ID('dbo.ChiTietKiemKe', 'U') IS NOT NULL DROP TABLE dbo.ChiTietKiemKe;
IF OBJECT_ID('dbo.PhieuKiemKe', 'U') IS NOT NULL DROP TABLE dbo.PhieuKiemKe;
IF OBJECT_ID('dbo.ChiTietNhapKho', 'U') IS NOT NULL DROP TABLE dbo.ChiTietNhapKho;
IF OBJECT_ID('dbo.PhieuNhapKho', 'U') IS NOT NULL DROP TABLE dbo.PhieuNhapKho;
IF OBJECT_ID('dbo.ChiTietGioHang', 'U') IS NOT NULL DROP TABLE dbo.ChiTietGioHang;
IF OBJECT_ID('dbo.GioHang', 'U') IS NOT NULL DROP TABLE dbo.GioHang;
IF OBJECT_ID('dbo.ChiTietHoaDon', 'U') IS NOT NULL DROP TABLE dbo.ChiTietHoaDon;
IF OBJECT_ID('dbo.HoaDon', 'U') IS NOT NULL DROP TABLE dbo.HoaDon;
IF OBJECT_ID('dbo.SanPhamTrangSuc', 'U') IS NOT NULL DROP TABLE dbo.SanPhamTrangSuc;
IF OBJECT_ID('dbo.NhomSanPham', 'U') IS NOT NULL DROP TABLE dbo.NhomSanPham;
IF OBJECT_ID('dbo.DanhMuc', 'U') IS NOT NULL DROP TABLE dbo.DanhMuc;
IF OBJECT_ID('dbo.NhanVien', 'U') IS NOT NULL DROP TABLE dbo.NhanVien;
IF OBJECT_ID('dbo.KhachHang', 'U') IS NOT NULL DROP TABLE dbo.KhachHang;
IF OBJECT_ID('dbo.NguoiDung', 'U') IS NOT NULL DROP TABLE dbo.NguoiDung;
IF OBJECT_ID('dbo.VaiTro', 'U') IS NOT NULL DROP TABLE dbo.VaiTro;
GO

-- ====================================================================================
-- PHẦN II: TẠO CẤU TRÚC 15 BẢNG QUAN HỆ (DDL)
-- ====================================================================================

-- 1. Bảng VaiTro: Định nghĩa các vai trò trong hệ thống (ADMIN, SALE, WAREHOUSE, KHACHHANG)
CREATE TABLE dbo.VaiTro (
    MaVaiTro VARCHAR(20) NOT NULL PRIMARY KEY,
    TenVaiTro NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(255) NULL
);
GO

-- 2. Bảng NguoiDung: Bảng cha cơ sở chứa tài khoản và định danh chung (Kế thừa mô hình TPT)
CREATE TABLE dbo.NguoiDung (
    MaNguoiDung VARCHAR(50) NOT NULL PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(20) NULL,
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhau NVARCHAR(255) NOT NULL,
    MaVaiTro VARCHAR(20) NOT NULL,
    TrangThai BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_NguoiDung_VaiTro FOREIGN KEY (MaVaiTro) REFERENCES dbo.VaiTro(MaVaiTro)
);
GO

-- 3. Bảng NhanVien: Kế thừa NguoiDung, lưu thông tin nhân sự nội bộ cửa hàng
CREATE TABLE dbo.NhanVien (
    MaNguoiDung VARCHAR(50) NOT NULL PRIMARY KEY,
    MaNhanVien VARCHAR(50) NOT NULL UNIQUE,
    LuongCoBan DECIMAL(18, 2) NOT NULL DEFAULT 0,
    CONSTRAINT FK_NhanVien_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung) ON DELETE CASCADE
);
GO

-- 4. Bảng KhachHang: Kế thừa NguoiDung, lưu thông tin khách hàng mua sắm
CREATE TABLE dbo.KhachHang (
    MaNguoiDung VARCHAR(50) NOT NULL PRIMARY KEY,
    MaKhachHang VARCHAR(50) NOT NULL UNIQUE,
    NgaySinh DATE NOT NULL,
    CONSTRAINT FK_KhachHang_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung) ON DELETE CASCADE
);
GO

-- 5. Bảng DanhMuc: Phân loại ngành hàng trang sức cao cấp
CREATE TABLE dbo.DanhMuc (
    MaDanhMuc VARCHAR(50) NOT NULL PRIMARY KEY,
    TenDanhMuc NVARCHAR(150) NOT NULL,
    MoTa NVARCHAR(500) NULL
);
GO

-- 5.1. Bảng NhomSanPham: Quản lý các nhóm sản phẩm cùng mẫu nhưng khác size
CREATE TABLE dbo.NhomSanPham (
    MaNhom NVARCHAR(50) NOT NULL PRIMARY KEY,
    TenNhom NVARCHAR(255) NOT NULL,
    MoTa NVARCHAR(500) NULL
);
GO

-- 6. Bảng SanPhamTrangSuc: Quản lý sản phẩm trang sức độc bản
CREATE TABLE dbo.SanPhamTrangSuc (
    MaSanPham VARCHAR(50) NOT NULL PRIMARY KEY,
    TenSanPham NVARCHAR(200) NOT NULL,
    MaDanhMuc VARCHAR(50) NOT NULL,
    MaNhom NVARCHAR(50) NOT NULL DEFAULT '0',
    ChatLieu NVARCHAR(100) NOT NULL,
    Size NVARCHAR(20) NULL,
    GiaNhap DECIMAL(18, 2) NOT NULL,
    GiaBan DECIMAL(18, 2) NOT NULL,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT 'HetHang', -- SanSang, HetHang, NgungKinhDoanh
    HinhAnh NVARCHAR(500) NULL,
    PhanTramGiamGia INT NOT NULL DEFAULT 0,
    TyLeLoiNhuan DECIMAL(5, 2) NOT NULL DEFAULT 0,
    CONSTRAINT FK_SanPham_DanhMuc FOREIGN KEY (MaDanhMuc) REFERENCES dbo.DanhMuc(MaDanhMuc)
);
GO

-- 7. Bảng HoaDon: Đơn đặt hàng và hóa đơn bán hàng tại quầy POS hoặc trực tuyến
CREATE TABLE dbo.HoaDon (
    MaHoaDon VARCHAR(50) NOT NULL PRIMARY KEY,
    NgayLap DATETIME2 NOT NULL DEFAULT GETDATE(),
    MaKhachHang VARCHAR(50) NOT NULL,
    MaNhanVien VARCHAR(50) NULL,
    DiaChiGiaoHang NVARCHAR(500) NOT NULL,
    SoDienThoaiNhan VARCHAR(20) NOT NULL,
    PhuongThucThanhToan NVARCHAR(50) NOT NULL, -- TienMat, ChuyenKhoan, COD, TheNganHang
    TrangThaiDonHang NVARCHAR(50) NOT NULL DEFAULT 'ChoDuyet', -- ChoDuyet, DaDuyet, DangGiao, DaGiao, DaHuy
    TongTienHang DECIMAL(18, 2) NOT NULL DEFAULT 0,
    TienGiamGia DECIMAL(18, 2) NOT NULL DEFAULT 0,
    TongThanhToan DECIMAL(18, 2) NOT NULL DEFAULT 0,
    GhiChu NVARCHAR(500) NULL,
    CONSTRAINT FK_HoaDon_KhachHang FOREIGN KEY (MaKhachHang) REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT FK_HoaDon_NhanVien FOREIGN KEY (MaNhanVien) REFERENCES dbo.NguoiDung(MaNguoiDung)
);
GO

-- 8. Bảng ChiTietHoaDon: Chi tiết từng món trang sức trong đơn hàng (Khóa chính kết hợp)
CREATE TABLE dbo.ChiTietHoaDon (
    SoHoaDon VARCHAR(50) NOT NULL,
    MaSanPham VARCHAR(50) NOT NULL,
    DonGia DECIMAL(18, 2) NOT NULL,
    SoLuong INT NOT NULL DEFAULT 1,
    PhanTramGiamGia INT NOT NULL DEFAULT 0,
    PRIMARY KEY (SoHoaDon, MaSanPham),
    CONSTRAINT FK_ChiTietHoaDon_HoaDon FOREIGN KEY (SoHoaDon) REFERENCES dbo.HoaDon(MaHoaDon) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietHoaDon_SanPham FOREIGN KEY (MaSanPham) REFERENCES dbo.SanPhamTrangSuc(MaSanPham)
);
GO

-- 9. Bảng GioHang: Giỏ hàng trực tuyến trên cổng Web khách hàng
CREATE TABLE dbo.GioHang (
    MaGioHang VARCHAR(50) NOT NULL PRIMARY KEY,
    MaKhachHang VARCHAR(50) NOT NULL UNIQUE,
    SoLuong INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_GioHang_KhachHang FOREIGN KEY (MaKhachHang) REFERENCES dbo.NguoiDung(MaNguoiDung) ON DELETE CASCADE
);
GO

-- 10. Bảng ChiTietGioHang: Sản phẩm nằm trong giỏ hàng (Khóa chính kết hợp)
CREATE TABLE dbo.ChiTietGioHang (
    MaGioHang VARCHAR(50) NOT NULL,
    MaSanPham VARCHAR(50) NOT NULL,
    SoLuongSanPham INT NOT NULL DEFAULT 1,
    PRIMARY KEY (MaGioHang, MaSanPham),
    CONSTRAINT FK_ChiTietGioHang_GioHang FOREIGN KEY (MaGioHang) REFERENCES dbo.GioHang(MaGioHang) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietGioHang_SanPham FOREIGN KEY (MaSanPham) REFERENCES dbo.SanPhamTrangSuc(MaSanPham)
);
GO

-- 11. Bảng PhieuNhapKho: Tiếp nhận nguyên liệu, trang sức nhập từ nhà chế tác
CREATE TABLE dbo.PhieuNhapKho (
    MaPhieuNhap VARCHAR(50) NOT NULL PRIMARY KEY,
    NgayNhap DATETIME2 NOT NULL DEFAULT GETDATE(),
    NhaCungCap NVARCHAR(200) NOT NULL,
    MaNhanVien VARCHAR(50) NOT NULL,
    TongSoSanPham INT NOT NULL DEFAULT 0,
    TongGiaTri DECIMAL(18, 2) NOT NULL DEFAULT 0,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT 'DaDuyet', -- ChoDuyet: Chờ Admin duyệt, DaDuyet: Đã duyệt (cộng kho), DaHuy: Đã hủy
    CONSTRAINT FK_PhieuNhapKho_NhanVien FOREIGN KEY (MaNhanVien) REFERENCES dbo.NguoiDung(MaNguoiDung)
);
GO

-- 12. Bảng ChiTietNhapKho: Dòng chi tiết nhập từng mã trang sức (Khóa chính kết hợp)
CREATE TABLE dbo.ChiTietNhapKho (
    MaPhieuNhap VARCHAR(50) NOT NULL,
    MaSanPham VARCHAR(50) NOT NULL,
    SoLuong INT NOT NULL,
    GiaVonNhap DECIMAL(18, 2) NOT NULL,
    SoChungTu NVARCHAR(100) NULL,
    PRIMARY KEY (MaPhieuNhap, MaSanPham),
    CONSTRAINT FK_ChiTietNhapKho_PhieuNhap FOREIGN KEY (MaPhieuNhap) REFERENCES dbo.PhieuNhapKho(MaPhieuNhap) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietNhapKho_SanPham FOREIGN KEY (MaSanPham) REFERENCES dbo.SanPhamTrangSuc(MaSanPham)
);
GO

-- 13. Bảng PhieuKiemKe: Đối soát tồn thực tế tại tủ trưng bày so với hệ thống
CREATE TABLE dbo.PhieuKiemKe (
    MaPhieuKiemKe VARCHAR(50) NOT NULL PRIMARY KEY,
    NgayKiemKe DATETIME2 NOT NULL DEFAULT GETDATE(),
    ViTriKiemKe NVARCHAR(150) NULL,
    MaNhanVienKho VARCHAR(50) NOT NULL,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT 'ChoDuyet', -- ChoDuyet, DaDuyet, VoHieuHoa
    TrangThaiDuyet BIT NOT NULL DEFAULT 0,
    GhiChu NVARCHAR(500) NULL,
    NgayDuyet DATETIME2 NULL,
    CONSTRAINT FK_PhieuKiemKe_NhanVien FOREIGN KEY (MaNhanVienKho) REFERENCES dbo.NguoiDung(MaNguoiDung)
);
GO

-- 14. Bảng ChiTietKiemKe: Chi tiết từng món trang sức kiểm kê (Khóa chính kết hợp)
CREATE TABLE dbo.ChiTietKiemKe (
    MaPhieuKiemKe VARCHAR(50) NOT NULL,
    MaSanPham VARCHAR(50) NOT NULL,
    TonSoSach INT NOT NULL DEFAULT 0,
    TonThucTe INT NOT NULL DEFAULT 0,
    ChenhLech INT NOT NULL DEFAULT 0,
    LyDoChenhLech NVARCHAR(300) NULL,
    PRIMARY KEY (MaPhieuKiemKe, MaSanPham),
    CONSTRAINT FK_ChiTietKiemKe_PhieuKiemKe FOREIGN KEY (MaPhieuKiemKe) REFERENCES dbo.PhieuKiemKe(MaPhieuKiemKe) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietKiemKe_SanPham FOREIGN KEY (MaSanPham) REFERENCES dbo.SanPhamTrangSuc(MaSanPham)
);
GO

-- 15. Bảng PhieuBaoHanh: Tiếp nhận làm mới, gắn lại đá quý, bảo dưỡng trang sức
CREATE TABLE dbo.PhieuBaoHanh (
    SoPhieuBH VARCHAR(50) NOT NULL PRIMARY KEY,
    MaKhachHang VARCHAR(50) NOT NULL,
    MaSanPham VARCHAR(50) NOT NULL,
    LyDoBaoHanh NVARCHAR(500) NOT NULL,
    TinhTrangTiepNhan NVARCHAR(300) NULL,
    GhiChu NVARCHAR(500) NULL,
    ChiPhiUocTinh DECIMAL(18, 2) NOT NULL DEFAULT 0,
    ChiPhiThucTe DECIMAL(18, 2) NOT NULL DEFAULT 0,
    ChiPhiBaoHanh DECIMAL(18, 2) NOT NULL DEFAULT 0,
    TrangThaiXuLy NVARCHAR(50) NOT NULL DEFAULT 'TiepNhan', -- TiepNhan, DangXuLy, DaXuLy, DaTraKhach, DaHuy
    SoDienThoai VARCHAR(20) NULL,
    NgayTiepNhan DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayHenTra DATETIME2 NULL,
    CONSTRAINT FK_PhieuBaoHanh_KhachHang FOREIGN KEY (MaKhachHang) REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT FK_PhieuBaoHanh_SanPham FOREIGN KEY (MaSanPham) REFERENCES dbo.SanPhamTrangSuc(MaSanPham)
);
GO

-- ====================================================================================
-- PHẦN III: DỮ LIỆU THỰC TẾ MẪU (DML SEED DATA KIM HOÀN CAO CẤP)
-- ====================================================================================

-- 1. Nạp vai trò hệ thống
INSERT INTO dbo.VaiTro (MaVaiTro, TenVaiTro, MoTa) VALUES
('ADMIN', N'Quản trị viên', N'Quản trị toàn quyền hệ thống cửa hàng kim hoàn'),
('SALE', N'Nhân viên bán hàng', N'Thu ngân và tư vấn bán hàng tại quầy POS'),
('WAREHOUSE', N'Thủ kho', N'Quản lý nhập kho và lập biên bản kiểm kê trang sức'),
('TECHNICIAN', N'Nhân viên kỹ thuật', N'Tiếp nhận sửa chữa, gia công và quyết toán chi phí bảo hành trang sức'),
('KHACHHANG', N'Khách hàng', N'Khách hàng mua sắm trực tuyến và tra cứu bảo hành');
GO

-- 2. Nạp người dùng (Bảng cha NguoiDung)
INSERT INTO dbo.NguoiDung (MaNguoiDung, HoTen, SoDienThoai, TenDangNhap, MatKhau, MaVaiTro, TrangThai) VALUES
('0', N'Khách Hàng Vãng Lai (POS)', '', 'khach.pos', '123456789', 'KHACHHANG', 1),
('USER_ADMIN01', N'Trần Minh Hoàng', '0901234567', 'admin', '123456789', 'ADMIN', 1),
('USER_NV001', N'Lê Phương Thảo', '0912345678', 'thao.le', 'pass123', 'SALE', 1),
('USER_NV002', N'Nguyễn Quốc Huy', '0933445566', 'huy.nguyen', 'pass123', 'WAREHOUSE', 1),
('USER_NV003', N'Đỗ Văn Kỹ Thuật', '0944556677', 'kythuat', '123456789', 'TECHNICIAN', 1),
('USER_KH001', N'Phạm Bích Ngọc', '0988112233', 'ngoc.pham', 'khach123', 'KHACHHANG', 1),
('USER_KH002', N'Hoàng Tuấn Anh', '0977223344', 'anh.hoang', 'khach123', 'KHACHHANG', 1);
GO

-- 3. Nạp thông tin nhân viên (Bảng con NhanVien)
INSERT INTO dbo.NhanVien (MaNguoiDung, MaNhanVien, LuongCoBan) VALUES
('USER_ADMIN01', 'NV_ADMIN', 25000000),
('USER_NV001', 'NV001', 12000000),
('USER_NV002', 'NV002', 14000000),
('USER_NV003', 'NV003', 10000000);
GO

-- 4. Nạp thông tin khách hàng (Bảng con KhachHang)
INSERT INTO dbo.KhachHang (MaNguoiDung, MaKhachHang, NgaySinh) VALUES
('0', 'KH_GUEST_POS', '2000-01-01'),
('USER_KH001', 'KH001', '1995-08-15'),
('USER_KH002', 'KH002', '1992-03-20');
GO

-- 5. Nạp danh mục trang sức cao cấp Blink Blink Jewelry
INSERT INTO dbo.DanhMuc (MaDanhMuc, TenDanhMuc, MoTa) VALUES
('DM00', N'Chưa phân loại', N'Danh mục mặc định cho sản phẩm chưa phân loại hoặc danh mục cũ bị xóa'),
('DM01', N'Nhẫn Kim Cương', N'Các dòng nhẫn đính hôn, nhẫn cưới và nhẫn kim cương tự nhiên tinh xảo'),
('DM02', N'Dây Chuyền & Mặt Dây', N'Dây chuyền vàng Ý 18K và mặt dây đính đá quý phong thủy'),
('DM03', N'Bông Tai Quý Phái', N'Bông tai đính đá Sapphire, Ruby và ngọc trai thiên nhiên thượng hạng'),
('DM04', N'Vòng Tay & Lắc Kim Cương', N'Lắc tay Tennis và vòng kiềng chế tác thủ công cao cấp');
GO

-- 5.1. Nạp nhóm sản phẩm mẫu (dòng sản phẩm đa dạng kích cỡ)
INSERT INTO dbo.NhomSanPham (MaNhom, TenNhom, MoTa) VALUES
('NHOM01', N'Bộ Sưu Tập Nhẫn Solitaire Diamond 18K', N'Dòng nhẫn kim cương đính chấu 4 chấu kinh điển với các kích cỡ size 14 và size 15');
GO

-- 6. Nạp sản phẩm trang sức độc bản
INSERT INTO dbo.SanPhamTrangSuc (MaSanPham, TenSanPham, MaDanhMuc, ChatLieu, Size, GiaNhap, GiaBan, TrangThai, HinhAnh, PhanTramGiamGia, MaNhom) VALUES
('SP001', N'Nhẫn Solitaire Diamond 18K', 'DM01', N'Vàng trắng 18K đính kim cương chủ 5.4 ly', N'Size 14', 18000000, 28500000, 'SanSang', 'https://images.unsplash.com/photo-1605100804763-247f67b3557e?auto=format&fit=crop&w=800&q=80', 10, 'NHOM01'),
('SP001_S15', N'Nhẫn Solitaire Diamond 18K (Size 15)', 'DM01', N'Vàng trắng 18K đính kim cương chủ 5.4 ly', N'Size 15', 18000000, 28500000, 'SanSang', 'https://images.unsplash.com/photo-1605100804763-247f67b3557e?auto=format&fit=crop&w=800&q=80', 10, 'NHOM01'),
('SP002', N'Dây Chuyền Eternal Heart Ruby', 'DM02', N'Vàng hồng 18K đính đá Ruby huyết bồ câu', N'Size 45cm', 12000000, 19200000, 'SanSang', 'https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?auto=format&fit=crop&w=800&q=80', 0, '0'),
('SP003', N'Bông Tai Royal Blue Sapphire', 'DM03', N'Bạch kim Platin 950 đính Sapphire Hoàng gia', N'Tiêu chuẩn', 22000000, 34000000, 'SanSang', 'https://images.unsplash.com/photo-1630019852942-f89202989a59?auto=format&fit=crop&w=800&q=80', 15, '0'),
('SP004', N'Lắc Tay Tennis Diamond Bracelet', 'DM04', N'Vàng trắng 18K đính 45 viên kim cương tự nhiên', N'Size 17cm', 48000000, 72000000, 'SanSang', 'https://images.unsplash.com/photo-1611591475152-47354900a359?auto=format&fit=crop&w=800&q=80', 20, '0'),
('SP005', N'Nhẫn Cưới Harmony Eternity', 'DM01', N'Vàng Ý 750 tinh tế đính kim cương tấm viền', N'Size 16', 11000000, 16800000, 'SanSang', 'https://images.unsplash.com/photo-1603561591411-07134e71a2a9?auto=format&fit=crop&w=800&q=80', 5, '0'),
('SP006', N'Mặt Dây Chuyền Ngọc Trai South Sea', 'DM02', N'Vàng vàng 18K đính ngọc trai biển và kim cương', N'Size 42cm', 28000000, 42500000, 'SanSang', 'https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?auto=format&fit=crop&w=800&q=80', 0, '0');
GO

-- 7. Nạp phiếu nhập kho mẫu
INSERT INTO dbo.PhieuNhapKho (MaPhieuNhap, NgayNhap, NhaCungCap, MaNhanVien, TongSoSanPham, TongGiaTri) VALUES
('PNK20260301', '2026-03-01 09:30:00', N'Tập đoàn Kim Hoàn Doji Atelier', 'USER_NV002', 10, 111000000);

INSERT INTO dbo.ChiTietNhapKho (MaPhieuNhap, MaSanPham, SoLuong, GiaVonNhap, SoChungTu) VALUES
('PNK20260301', 'SP001', 3, 18000000, 'CT-DOJI-991'),
('PNK20260301', 'SP001_S15', 2, 18000000, 'CT-DOJI-991B'),
('PNK20260301', 'SP002', 3, 12000000, 'CT-DOJI-992'),
('PNK20260301', 'SP003', 3, 22000000, 'CT-DOJI-994'),
('PNK20260301', 'SP004', 3, 48000000, 'CT-DOJI-995'),
('PNK20260301', 'SP005', 4, 11000000, 'CT-DOJI-993'),
('PNK20260301', 'SP006', 2, 28000000, 'CT-DOJI-996');
GO

-- 8. Nạp hóa đơn bán hàng thực tế
INSERT INTO dbo.HoaDon (MaHoaDon, NgayLap, MaKhachHang, MaNhanVien, DiaChiGiaoHang, SoDienThoaiNhan, PhuongThucThanhToan, TrangThaiDonHang, TongTienHang, TienGiamGia, TongThanhToan, GhiChu) VALUES
('HD1001', '2026-03-10 14:15:00', 'USER_KH001', 'USER_NV001', N'45 Lê Lợi, Phường Bến Nghé, Quận 1, TP.HCM', '0988112233', N'ChuyenKhoan', N'DaGiao', 28500000, 2850000, 25650000, N'Khách mua kỷ niệm ngày cưới, đóng gói hộp nhung cao cấp'),
('HD1002', '2026-03-15 16:45:00', 'USER_KH002', 'USER_NV001', N'128 Hai Bà Trưng, Phường Đa Kao, Quận 1, TP.HCM', '0977223344', N'TheNganHang', N'DaGiao', 19200000, 0, 19200000, N'Giao hàng hỏa tốc trong ngày'),
('HD1003', '2026-03-20 10:20:00', 'USER_KH001', NULL, N'45 Lê Lợi, Phường Bến Nghé, Quận 1, TP.HCM', '0988112233', N'ChuyenKhoan', N'ChoDuyet', 34000000, 5100000, 28900000, N'Khách đặt trực tuyến qua cổng Web khách hàng');

INSERT INTO dbo.ChiTietHoaDon (SoHoaDon, MaSanPham, DonGia, SoLuong, PhanTramGiamGia) VALUES
('HD1001', 'SP001', 28500000, 1, 10),
('HD1002', 'SP002', 19200000, 1, 0),
('HD1003', 'SP003', 34000000, 1, 15);
GO

-- 9. Nạp giỏ hàng trực tuyến mẫu cho khách hàng
INSERT INTO dbo.GioHang (MaGioHang, MaKhachHang, SoLuong) VALUES
('GH_KH001', 'USER_KH001', 1),
('GH_KH002', 'USER_KH002', 0);

INSERT INTO dbo.ChiTietGioHang (MaGioHang, MaSanPham, SoLuongSanPham) VALUES
('GH_KH001', 'SP004', 1);
GO

-- 10. Phiếu kiểm kê kho (để trống ban đầu, nhân viên kho tự tạo khi kiểm kê)
GO

-- 11. Nạp phiếu tiếp nhận bảo hành dịch vụ kim hoàn
INSERT INTO dbo.PhieuBaoHanh (SoPhieuBH, MaKhachHang, MaSanPham, LyDoBaoHanh, ChiPhiBaoHanh, TrangThaiXuLy, NgayTiepNhan, NgayHenTra) VALUES
('BH2001', 'USER_KH001', 'SP001', N'Làm sạch, đánh bóng siêu âm và kiểm tra chấu giữ viên kim cương chủ', 0, N'DaHoanTat', '2026-03-12 10:00:00', '2026-03-14 17:00:00'),
('BH2002', 'USER_KH002', 'SP002', N'Đứt khoen móc khóa cài dây chuyền vàng hồng, yêu cầu hàn laser thẩm mỹ', 150000, N'DangBaoHanh', '2026-03-18 15:30:00', '2026-03-22 17:00:00');
GO

PRINT N'>>> HOÀN TẤT KHỞI TẠO CƠ SỞ DỮ LIỆU BLINK BLINK JEWELRY VÀ NẠP DỮ LIỆU MẪU THÀNH CÔNG! <<<';
GO
