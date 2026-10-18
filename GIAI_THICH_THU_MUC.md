# GIẢI THÍCH CHI TIẾT CẤU TRÚC THƯ MỤC DỰ ÁN BLINK BLINK JEWELRY

Tài liệu này giải thích cụ thể: **mỗi thư mục hiện tại chứa gì** và **trong tương lai sẽ được viết thêm những gì**, bám sát 100% bản vẽ thiết kế `Class Diagram.xml` và 5 sơ đồ phân hệ chi tiết trong thư mục `XML/`.

---

## 1. THƯ MỤC NGUỒN CHÍNH: `src/`

Chứa toàn bộ mã nguồn của hệ thống, tổ chức theo kiến trúc phân tầng (Clean Architecture) gồm 5 dự án thành phần:

---

### 1.1. `src/BlinkBlink.Domain/` (Tầng Lõi Nghiệp Vụ - Độc Lập Hoàn Toàn)
Là trái tim của hệ thống. Tầng này chứa các thực thể và quy tắc kinh doanh của cửa hàng kim hoàn, không phụ thuộc vào CSDL hay giao diện.

* **Hiện tại đang có:**
  * `Common/`: Các lớp nền tảng dùng chung.
  * `Entities/`: Phân chia thành các thư mục nghiệp vụ chuẩn hóa theo Class Diagram:
    * `Identity/`: Quản lý tài khoản, nhân viên, phân quyền và nhật ký kiểm toán.
    * `Catalog/`: Quản lý danh mục, thông tin trang sức và lịch sử giá.
    * `Orders/`: Quản lý hóa đơn bán hàng tại quầy và trực tuyến.
    * `Cart/`: Quản lý giỏ hàng trực tuyến cho khách mua sắm trên Web.
    * `Customers/`: Quản lý thông tin khách hàng.
    * `Inventory/`: Quản lý phiếu nhập kho và biên bản kiểm kê định kỳ.
    * `Warranty/`: Quản lý phiếu tiếp nhận và bảo hành trang sức.
  * `Enums/`: Thư mục chứa các kiểu liệt kê trạng thái.
  * `Exceptions/`: Thư mục chứa các ngoại lệ nghiệp vụ riêng.
  * `Interfaces/`: Hợp đồng truy vấn dữ liệu (Repository Interfaces).

* **Danh sách lớp thực thể cụ thể (Chuẩn hóa 100% theo bản vẽ Diagram):**
  * `Identity/`:
    * `VaiTro.cs`: `MaVaiTro`, `TenVaiTro`, `MoTa`, `ThemQuyen(maQuyen)`, `XoaQuyen(maQuyen)`.
    * `NguoiDung.cs` (lớp trừu tượng `<<abstract>>`): `MaNguoiDung`, `HoTen`, `SoDienThoai`, `TenDangNhap`, `MatKhau`, `MaVaiTro`, `TrangThai`, `DangNhap(tenDN, mk)`, `DoiMatKhau(mkMoi)`, `CapNhatThongTin(hoTen, sdt)`, `KhoaTaiKhoan()`.
    * `NhanVien.cs` (kế thừa `NguoiDung`): `MaNhanVien`, `LuongCoBan`.
    * `AuditLog.cs`: `MaLog`, `MaNhanVien`, `HanhDong`, `DoiTuongTacDong`, `ThoiGian`, `NoiDungChiTiet`.
  * `Catalog/`:
    * `DanhMuc.cs`: `MaDanhMuc`, `TenDanhMuc`, `MoTa`, `LayDanhSachSanPham()`, `CapNhatDanhMuc()`.
    * `SanPhamTrangSuc.cs`: `MaSanPham`, `TenSanPham`, `MaDanhMuc`, `ChatLieu`, `Size`, `TrangThai`, `GiaNhap`, `GiaBan`.
    * `LichSuGia.cs`: `MaLichSu`, `MaSanPham`, `GiaNhapCu`, `GiaNhapMoi`, `GiaBanCu`, `GiaBanMoi`, `NgayThayDoi`, `MaNhanVienThucHien`.
  * `Orders/`:
    * `HoaDon.cs`: `SoHoaDon`, `NgayLap`, `MaNhanVien`, `MaKhachHang`, `TongTienHang`, `TienGiamGia`, `TongThanhToan`.
    * `ChiTietHoaDon.cs`: `SoHoaDon`, `MaSanPham`, `ThanhTien`.
  * `Cart/`:
    * `GioHang.cs`: `MaGioHang`, `MaKhachHang`, `SoLuong`.
    * `ChiTietGioHang.cs`: `MaGioHang`, `MaSanPham`, `SoLuongSanPham`.
  * `Customers/`:
    * `KhachHang.cs` (kế thừa `NguoiDung`): `MaKhachHang`, `NgaySinh`, `ToString()`.
  * `Inventory/`:
    * `PhieuNhapKho.cs`: `SoPhieuNhap`, `NgayNhap`, `NhaCungCap`, `MaNhanVien`, `TongSoSanPham`, `TongGiaTri`.
    * `ChiTietNhapKho.cs`: `SoPhieuNhap`, `MaSanPham`, `SoLuong`, `GiaVonNhap`, `SoChungTu`.
    * `PhieuKiemKe.cs`: `SoPhieuKiemKe`, `NgayKiemKe`, `ViTriKiemKe`, `MaNhanVienKho`, `TrangThaiDuyet`.
    * `ChiTietKiemKe.cs`: `SoPhieuKiemKe`, `MaSanPham`, `TonSoSach`, `TonThucTe`, `ChenhLech`.
  * `Warranty/`:
    * `PhieuBaoHanh.cs`: `SoPhieuBH`, `MaKhachHang`, `MaSanPham`, `DichVuYeuCau`, `ChiPhiSuaChua`, `TrangThaiXuLy`.
  * `Interfaces/` (Repository Interfaces chuẩn hóa):
    * `INguoiDungRepository.cs`, `INhanVienRepository.cs`, `IKhachHangRepository.cs`, `IVaiTroRepository.cs`.
    * `ISanPhamRepository.cs`, `IDanhMucRepository.cs`, `IPhieuNhapKhoRepository.cs`.
    * `IHoaDonRepository.cs`, `IGioHangRepository.cs`.
    * `IBaoHanhRepository.cs`.
    * `IThongKeRepository.cs`.

---

### 1.2. `src/BlinkBlink.Application/` (Tầng Ca Sử Dụng & Dịch Vụ Nghiệp Vụ)
Điều phối luồng công việc giữa giao diện và cơ sở dữ liệu. Nhận yêu cầu từ người dùng thông qua các DTO, thực thi nghiệp vụ trong Use Cases và tương tác qua các Repository Interfaces.

* **Cấu trúc thư mục chuẩn hóa:**
  * `DTOs/`: Chứa các Data Transfer Objects truyền nhận dữ liệu:
    * `Identity/`: `DangNhapDTO.cs`, `DangKyKhachHangDTO.cs`, `TaoNhanVienDTO.cs`.
    * `Orders/`: `DatHangDTO.cs`.
    * `Inventory/`: `SanPhamDTO.cs`, `NhapKhoDTO.cs`.
    * `Warranty/`: `TaoBaoHanhDTO.cs`, `CapNhatTrangThaiDTO.cs`.
    * `Reporting/`: `DoanhThuThongKeDTO.cs`, `BaoCaoTonKhoDTO.cs`, `TopSanPhamBanChayDTO.cs`.
  * `Services/` (Use Cases tương ứng 5 phân hệ theo Diagram):
    * `Identity/`:
      * `DangNhapUseCase.cs`: Xác thực tài khoản người dùng đăng nhập hệ thống.
      * `DangKyKhachHangUseCase.cs`: Đăng ký tài khoản khách hàng mới trên Web MVC.
      * `QuanLyNhanVienUseCase.cs`: Thêm mới nhân sự, cấp vai trò và khóa tài khoản nhân viên.
    * `Orders/`:
      * `DatHangUseCase.cs`: Xử lý lưu đơn hàng, kiểm tra giỏ hàng và thanh toán.
    * `Inventory/`:
      * `QuanLySanPhamUseCase.cs`: Quản lý danh mục, thêm mới và cập nhật sản phẩm trang sức.
      * `XacNhanNhapKhoUseCase.cs`: Tiếp nhận hàng nhập kho từ nhà cung cấp và cộng tồn kho.
    * `Warranty/`:
      * `TiepNhanBaoHanhUseCase.cs`: Lập phiếu tiếp nhận sửa chữa, đánh bóng trang sức.
      * `XuLyBaoHanhUseCase.cs`: Cập nhật tiến độ hoàn thành và chi phí sửa chữa.
    * `Reporting/`:
      * `ThongKeDoanhThuUseCase.cs`: Tổng hợp doanh số bán hàng theo mốc thời gian.
      * `ThongKeKhoHangUseCase.cs`: Đối soát lượng tồn thực tế vs sổ sách và cảnh báo tồn kho.

---

### 1.3. `src/BlinkBlink.Infrastructure/` (Tầng Hạ Tầng & Cơ Sở Dữ Liệu SQL Server)
Chịu trách nhiệm kết nối trực tiếp với Microsoft SQL Server qua Entity Framework Core 8.

* **Cấu trúc thư mục chuẩn hóa:**
  * `Data/`:
    * `AppDbContext.cs`: Khai báo các `DbSet<T>` và tự động nạp cấu hình qua `ApplyConfigurationsFromAssembly`.
    * `Configurations/`: Các tệp cấu hình Fluent API độc lập cho từng bảng (`VaiTroConfiguration.cs`, `NguoiDungConfiguration.cs`, `NhanVienConfiguration.cs`, `KhachHangConfiguration.cs`...). Áp dụng chiến lược Table-per-Type (TPT) cho cây kế thừa `NguoiDung`.
  * `Repositories/`: Triển khai cụ thể các Repository kết nối SQL Server qua `AppDbContext`:
    * `NguoiDungRepository.cs`, `NhanVienRepository.cs`, `KhachHangRepository.cs`, `VaiTroRepository.cs`.
    * `SanPhamRepository.cs`, `DanhMucRepository.cs`, `PhieuNhapKhoRepositoryImpl.cs`.
    * `HoaDonRepository.cs`, `GioHangRepository.cs`.
    * `BaoHanhRepository.cs`.
    * `ThongKeRepository.cs`.

---

### 1.4. `src/BlinkBlink.Admin/` (Giao Diện Quản Trị & POS - Desktop WPF)
Ứng dụng máy tính Windows tại quầy dành cho Quản lý, Thu ngân và Thủ kho. Áp dụng mô hình **MVVM** và kiến trúc **UserControl** theo đúng yêu cầu giảng viên:

* **Cấu trúc thư mục:**
  * `MainWindow.xaml`: Cửa sổ khung điều hướng chính.
  * `Views/UserControls/`: Các màn hình nghiệp vụ độc lập:
    * `Identity/UC_NhanVien.xaml`: Quản lý nhân viên, phân quyền chức vụ.
    * `Orders/UC_BanHangPOS.xaml`: Bán hàng tại quầy, quét mã sản phẩm và in hóa đơn.
    * `Catalog/UC_SanPham.xaml` & `UC_LichSuGia.xaml`: Quản lý thông tin trang sức và xem biến động giá.
    * `Inventory/UC_NhapKho.xaml` & `UC_KiemKe.xaml`: Lập phiếu nhập kho, đối soát tồn sổ sách vs tồn thực tế.
    * `Customers/UC_KhachHang.xaml`: Tra cứu hồ sơ khách hàng.
    * `Warranty/UC_BaoHanh.xaml`: Nhập thông tin bảo hành, tiến độ sửa chữa.
    * `Reporting/UC_BaoCaoThongKe.xaml`: Màn hình thống kê doanh thu và kho hàng.
  * `Views/Windows/`: `LoginWindow.xaml` (Cửa sổ đăng nhập Desktop).
  * `ViewModels/`: Các lớp ViewModel liên kết dữ liệu (Data Binding) cho từng UserControl.

---

### 1.5. `src/BlinkBlink.Web/` (Cổng Thông Tin Cho Khách Hàng - Web ASP.NET Core MVC)
Website dành cho khách hàng bên ngoài truy cập bằng trình duyệt internet. Dùng chung tầng `Application` và CSDL SQL Server với ứng dụng Desktop.

* **Cấu trúc Controllers chuẩn hóa theo Diagram XML:**
  * `XacThucController.cs`: Xử lý đăng nhập khách hàng (`DangNhap`).
  * `KhachHangController.cs`: Xử lý đăng ký tài khoản khách hàng (`DangKy`).
  * `NhanVienController.cs`: Cấp tài khoản và khóa tài khoản nhân viên.
  * `BanHangController.cs`: Đặt hàng và xử lý giỏ hàng trực tuyến.
  * `SanPhamController.cs`: Xem danh mục trang sức, chi tiết sản phẩm.
  * `PhieuNhapKhoController.cs`: Tiếp nhận nhập kho.
  * `BaoHanhController.cs`: Tra cứu tiến độ bảo hành theo mã sản phẩm hoặc số phiếu.
  * `ThongKeController.cs`: Báo cáo thống kê.
* **Views/ (Giao diện Razor HTML/CSS):**
  * `XacThuc/DangNhap.cshtml`: Giao diện trang Đăng nhập sang trọng.
  * `KhachHang/DangKy.cshtml`: Giao diện trang Đăng ký thành viên mới.
  * `Home/Index.cshtml`: Trang chủ giới thiệu thương hiệu kim hoàn.

---

## 2. THƯ MỤC KIỂM THỬ: `tests/`

* **`tests/BlinkBlink.UnitTests/`**: Kiểm thử đơn vị các phương thức nghiệp vụ Domain (`DangNhap`, `DoiMatKhau`, tính tiền hóa đơn, kiểm kê kho).
* **`tests/BlinkBlink.IntegrationTests/`**: Kiểm thử tích hợp tương tác CSDL SQL Server qua EF Core 8.

---

## 3. CÁC TỆP TIN Ở THƯ MỤC GỐC (ROOT) DỰ ÁN
* **`BlinkBlinkJewelry.sln`**: Tệp Solution gốc của .NET 8 LTS quản lý toàn bộ 7 projects trong hệ thống Clean Architecture.
* **`GIAI_THICH_THU_MUC.md`**: Bản giải thích chi tiết cấu trúc thư mục, trách nhiệm từng tầng và phân công module.
* **`CONG_NGHE_VA_QUY_TRINH.md`**: Bản mô tả công nghệ sử dụng, kiến trúc phân tầng C4 và chu trình Migrations CSDL dùng chung.
* **`WORKFLOW.md`**: Quy ước đặt tên nhánh, commit message và quy trình phối hợp làm việc nhóm trên GitHub.

