# GIẢI THÍCH CHI TIẾT CẤU TRÚC THƯ MỤC DỰ ÁN BLINK BLINK JEWELRY

Tài liệu này giải thích cụ thể: **mỗi thư mục hiện tại chứa gì** và **trong tương lai sẽ được viết thêm những gì**, bám sát 100% bản vẽ thiết kế `Class Diagram.xml` và yêu cầu kiến trúc của giảng viên.

---

## 1. THƯ MỤC NGUỒN CHÍNH: `src/`

Chứa toàn bộ mã nguồn của hệ thống, tổ chức theo kiến trúc phân tầng (Clean Architecture) gồm 5 dự án thành phần:

---

### 1.1. `src/BlinkBlink.Domain/` (Tầng Lõi Nghiệp Vụ - Độc Lập Hoàn Toàn)
Là trái tim của hệ thống. Tầng này chứa các thực thể và quy tắc kinh doanh của cửa hàng kim hoàn, không phụ thuộc vào CSDL hay giao diện.

* **Hiện tại đang có:**
  * `Common/`: Các lớp nền tảng dùng chung (`BaseEntity.cs` định nghĩa ID, ngày tạo).
  * `Entities/`: Đã phân chia thành 7 thư mục nghiệp vụ tương ứng với 18 lớp trong `Class Diagram.xml`:
    * `Identity/`: Quản lý tài khoản, nhân viên, phân quyền và nhật ký kiểm toán.
    * `Catalog/`: Quản lý danh mục và thông tin trang sức.
    * `Orders/`: Quản lý hóa đơn bán hàng tại quầy.
    * `Cart/`: Quản lý giỏ hàng trực tuyến cho khách trên Web.
    * `Customers/`: Quản lý khách hàng và thẻ thành viên VIP.
    * `Inventory/`: Quản lý nhập kho và kiểm kê định kỳ.
    * `Warranty/`: Quản lý tiếp nhận và bảo hành trang sức.
  * `Enums/`: Thư mục chứa các kiểu liệt kê trạng thái.
  * `Exceptions/`: Thư mục chứa các biệt lệ nghiệp vụ riêng.
  * `Interfaces/`: Hợp đồng truy vấn dữ liệu (`IRepository<T>`, `IUnitOfWork`).

* **Tương lai sẽ viết thêm:**
  * **18 Lớp thực thể cụ thể (chuẩn hóa theo `Class Diagram.xml`):**
    * `Identity/`:
      * `VaiTro.cs`: `maVaiTro`, `tenVaiTro`, `moTa`.
      * `NguoiDung.cs` (giao diện `<<interface>>`): `maNguoiDung`, `tenDangNhap`, `matKhau`, `maVaiTro`, `trangThai`.
      * `NhanVien.cs`: `maNhanVien`, `hoTen`, `soDienThoai`, `maNguoiDung`, `tyLeHoaHong`.
      * `AuditLog.cs`: `maLog`, `maNhanVien`, `hanhDong`, `doiTuongTacDong`, `thoiGian`, `noiDungChiTiet`.
    * `Catalog/`:
      * `DanhMuc.cs`: `maDanhMuc`, `tenDanhMuc`, `moTa`.
      * `SanPhamTrangSuc.cs`: `maSanPham`, `tenSanPham`, `maDanhMuc`, `chatLieu`, `size`, `trangThai`, `giaNhap`, `giaBan`.
      * `LichSuGia.cs`: `maLichSu`, `maSanPham`, `giaNhapCu`, `giaNhapMoi`, `giaBanCu`, `giaBanMoi`, `ngayThayDoi`, `maNhanVienThucHien`.
    * `Orders/`:
      * `HoaDon.cs`: `soHoaDon`, `ngayLap`, `maNhanVien`, `maKhachHang`, `tongTienHang`, `tienGiamGia`, `tongThanhToan`.
      * `ChiTietHoaDon.cs`: `soHoaDon`, `maSanPham`, `thanhTien`.
    * `Cart/`:
      * `GioHang.cs`: `maGioHang`, `maKhachHang`, `soLuong`, `danhSachSanPham`.
      * `ChiTietGioHang.cs`: `maGioHang`, `maSanPham`, `soLuongSanPham`.
    * `Customers/`:
      * `TheThanhVien.cs`: `maHangThanhVien`, `tenHang`, `mocDiemYeuCau`, `phanTramGiamTienCong`.
      * `KhachHang.cs`: `maKhachHang`, `hoTen`, `soDienThoai`, `ngaySinh`, `diemTichLuy`, `maHangThanhVien`.
    * `Inventory/`:
      * `PhieuNhapKho.cs`: `soPhieuNhap`, `ngayNhap`, `nhaCungCap`, `maNhanVien`, `tongSoSanPham`, `tongGiaTri`.
      * `ChiTietNhapKho.cs`: `soPhieuNhap`, `maSanPham`, `soLuong`, `giaVonNhap`, `soChungTu`.
      * `PhieuKiemKe.cs`: `soPhieuKiemKe`, `ngayKiemKe`, `viTriKiemKe`, `maNhanVienKho`, `trangThaiDuyet`.
      * `ChiTietKiemKe.cs`: `soPhieuKiemKe`, `maSanPham`, `tonSoSach`, `tonThucTe`, `chenhLech`.
    * `Warranty/`:
      * `PhieuBaoHanh.cs`: `soPhieuBH`, `maKhachHang`, `maSanPham`, `dichVuYeuCau`, `chiPhiSuaChua`, `trangThaiXuLy`.
  * `Enums/`: `TrangThaiSanPham.cs`, `TrangThaiHoaDon.cs`, `TrangThaiKiemKe.cs`, `TrangThaiBaoHanh.cs`.
  * `Exceptions/`: `SanPhamDaBanException.cs`, `KhongDuTonKhoException.cs`, `NguoiDungBiKhoaException.cs`.

---

### 1.2. `src/BlinkBlink.Application/` (Tầng Ca Sử Dụng & Dịch Vụ Nghiệp Vụ)
Điều phối luồng công việc giữa giao diện và cơ sở dữ liệu. Nhận yêu cầu từ người dùng, gọi các thực thể Domain xử lý và lưu kết quả.

* **Hiện tại đang có:**
  * `Common/`: Các lớp truyền tải dữ liệu (DTO).
  * `Interfaces/`: Các hợp đồng dịch vụ (`IOrderService`, `IInventoryService`...).
  * `Services/`: Đã tạo sẵn 7 thư mục dịch vụ tương ứng 7 phân hệ:
    * `Identity/`, `Catalog/`, `Orders/`, `Cart/`, `Customers/`, `Inventory/`, `Warranty/`.
  * `Strategies/`: Các chiến lược giảm giá theo thẻ thành viên (Strategy Pattern).
  * `Validators/`: Các lớp kiểm tra tính hợp lệ của dữ liệu đầu vào.

* **Tương lai sẽ viết thêm:**
  * `Services/Orders/OrderService.cs`: Xử lý tạo đơn hàng tại quầy (POS), tính tiền chiết khấu thành viên, lưu hóa đơn và cập nhật trạng thái món trang sức.
  * `Services/Catalog/ProductService.cs` & `PriceHistoryService.cs`: Quản lý sản phẩm, cập nhật giá mới và tự động lưu vết vào `LichSuGia`.
  * `Services/Cart/CartService.cs`: Xử lý thêm, bớt, cập nhật số lượng món hàng trong giỏ hàng web.
  * `Services/Inventory/StockInService.cs` & `StockCheckService.cs`: Lập phiếu nhập kho từ nhà cung cấp và đối soát kiểm kê kho định kỳ.
  * `Services/Customers/CustomerService.cs`: Tự động cộng điểm thưởng cho khách và xét nâng hạng thẻ thành viên VIP khi đạt mốc.
  * `Services/Warranty/WarrantyService.cs`: Lập phiếu tiếp nhận sửa chữa, đánh bóng, theo dõi tiến độ và tính chi phí bảo hành.
  * `Services/Identity/AuthService.cs` & `AuditLogService.cs`: Xác thực đăng nhập, kiểm tra vai trò và ghi nhật ký thao tác người dùng.
  * `Strategies/`: `BronzeDiscountStrategy.cs`, `GoldDiscountStrategy.cs`, `DiamondDiscountStrategy.cs` (áp dụng giảm giá tiền công theo thẻ).

---

### 1.3. `src/BlinkBlink.Infrastructure/` (Tầng Hạ Tầng & Cơ Sở Dữ Liệu SQL Server)
Chịu trách nhiệm kết nối trực tiếp với Microsoft SQL Server qua Entity Framework Core 8.

* **Hiện tại đang có:**
  * `Data/`: Chứa cấu hình kết nối CSDL và các bản ghi cấu trúc bảng (Migrations).
  * `Repositories/`: Chứa các lớp truy vấn dữ liệu từ SQL Server.
  * `Reporting/`: Chuẩn bị định dạng dữ liệu hóa đơn điện tử để hiển thị ra màn hình.
  * `Security/`: Mã hóa mật khẩu và bảo mật thông tin.

* **Tương lai sẽ viết thêm:**
  * `Data/AppDbContext.cs`: Khai báo 18 bảng CSDL (`DbSet<T>`) tương ứng với 18 thực thể.
  * `Data/Configurations/`: Cấu hình Fluent API chi tiết cho 18 bảng (khóa chính, khóa ngoại, kiểu dữ liệu, chỉ mục Index tăng tốc độ truy vấn, bảo đảm chuẩn hóa 3NF).
  * `Data/Migrations/`: Các tệp mã nguồn tự động sinh và cập nhật bảng trên SQL Server.
  * `Repositories/GenericRepository<T>.cs`: Các phương thức truy vấn dùng chung (Thêm, Sửa, Xóa, Tìm kiếm theo mã ID).
  * `Security/PasswordHasher.cs`: Băm mật khẩu người dùng trước khi lưu vào CSDL, không lưu mật khẩu thô.
  * `Reporting/InvoicePreviewService.cs`: Định dạng dữ liệu hóa đơn bán lẻ để hiển thị trực tiếp lên màn hình máy tính cho thu ngân và khách xem trước.

---

### 1.4. `src/BlinkBlink.Admin/` (Giao Diện Quản Trị & POS - Desktop WPF)
Ứng dụng máy tính dành cho Quản lý, Thu ngân và Thủ kho. Áp dụng mô hình **MVVM** và kiến trúc **UserControl** theo đúng yêu cầu của giảng viên: cửa sổ chính `MainWindow.xaml` đóng vai trò khung điều hướng (Navigation Container), nội dung từng chức năng được tách thành các `UserControl` độc lập, dễ tái sử dụng và kiểm thử.

* **Hiện tại đang có:**
  * `App.xaml`, `App.xaml.cs`: Khởi động ứng dụng và đăng ký dịch vụ (Dependency Injection).
  * `MainWindow.xaml`: Cửa sổ làm việc chính chứa thanh menu điều hướng và vùng hiển thị UserControl.
  * `Views/UserControls/`: Đã tạo sẵn 7 thư mục chức năng:
    * `Orders/`: Giao diện bán hàng tại quầy (POS).
    * `Catalog/`: Giao diện quản lý danh mục và sản phẩm.
    * `Inventory/`: Giao diện lập phiếu nhập kho và kiểm kê.
    * `Customers/`: Giao diện hồ sơ khách hàng và hạng thẻ thành viên.
    * `Warranty/`: Giao diện tiếp nhận và quản lý bảo hành.
    * `Identity/`: Giao diện quản lý nhân viên, phân quyền và nhật ký thao tác.
    * `Reporting/`: Giao diện thống kê doanh thu và hoa hồng.
  * `Views/Windows/`: Chứa các cửa sổ độc lập (như cửa sổ đăng nhập `LoginWindow.xaml`).
  * `ViewModels/`: Đã tạo sẵn 7 thư mục ViewModel tương ứng với từng phân hệ trên để xử lý logic hiển thị và liên kết dữ liệu (Data Binding).
  * `Services/`: Dịch vụ điều hướng trang (`NavigationService.cs`) và hộp thoại thông báo.

* **Tương lai sẽ viết thêm:**
  * `Views/UserControls/Orders/UC_BanHangPOS.xaml`: Bán hàng tại quầy, tra cứu mã sản phẩm, tự động tính tổng tiền, giảm giá theo hạng thẻ, hiển thị hóa đơn xem trước.
  * `Views/UserControls/Catalog/UC_SanPham.xaml` & `UC_LichSuGia.xaml`: Quản lý thông tin trang sức và xem biến động giá nhập/bán.
  * `Views/UserControls/Inventory/UC_NhapKho.xaml` & `UC_KiemKe.xaml`: Lập phiếu nhập kho, đối soát tồn sổ sách vs tồn thực tế để lập biên bản chênh lệch.
  * `Views/UserControls/Customers/UC_KhachHang.xaml`: Tra cứu điểm tích lũy, lịch sử mua sắm và xét hạng thẻ.
  * `Views/UserControls/Warranty/UC_BaoHanh.xaml`: Nhập thông tin bảo hành, tình trạng tiếp nhận, chi phí sửa chữa.
  * `Views/UserControls/Identity/UC_NhanVien.xaml` & `UC_AuditLog.xaml`: Quản lý nhân viên và xem vết kiểm toán.
  * `Views/Windows/LoginWindow.xaml`: Cửa sổ đăng nhập kiểm tra tài khoản, mật khẩu và chuyển hướng theo vai trò.

---

### 1.5. `src/BlinkBlink.Web/` (Cổng Thông Tin Cho Khách Hàng - Web ASP.NET Core MVC)
Website dành cho khách hàng bên ngoài truy cập bằng trình duyệt internet. Dùng chung tầng `Application` và CSDL SQL Server với ứng dụng Desktop.

* **Hiện tại đang có:**
  * `Program.cs`: Cấu hình máy chủ web, định tuyến URL và kết nối dịch vụ.
  * `Controllers/`: Chứa các bộ điều khiển tiếp nhận yêu cầu từ trình duyệt.
  * `Views/`: Chứa giao diện trang web (HTML/Razor).
  * `Models/`: Các lớp dữ liệu dành riêng cho giao diện web.
  * `wwwroot/`: Chứa CSS, JavaScript, thư viện giao diện và hình ảnh sản phẩm.

* **Tương lai sẽ viết thêm:**
  * `Controllers/HomeController.cs`: Trang chủ giới thiệu thương hiệu và các bộ sưu tập nổi bật.
  * `Controllers/ProductsController.cs`: Xem danh mục trang sức, bộ lọc theo chất liệu/size/khoảng giá và trang chi tiết sản phẩm.
  * `Controllers/CartController.cs`: Xem giỏ hàng trực tuyến, thêm/bớt số lượng món đồ và gửi yêu cầu đặt hàng.
  * `Controllers/WarrantyController.cs`: Trang tra cứu tiến độ bảo hành theo số phiếu hoặc mã sản phẩm.
  * `Controllers/AccountController.cs`: Trang cho khách đăng nhập, xem thông tin thẻ thành viên và số điểm tích lũy của mình.

---

## 2. THƯ MỤC KIỂM THỬ: `tests/`

Chứa các đoạn mã tự động kiểm tra tính đúng đắn của logic nghiệp vụ trước khi đưa vào vận hành:

* **`tests/BlinkBlink.UnitTests/`**:
  * Kiểm tra tính tiền hóa đơn: tiền hàng trừ tiền giảm giá có đúng bằng tổng thanh toán hay không.
  * Kiểm tra công thức tính hoa hồng của nhân viên bán hàng.
  * Kiểm tra logic tự động nâng hạng thẻ thành viên khi điểm tích lũy vượt mốc quy định.
* **`tests/BlinkBlink.IntegrationTests/`**:
  * Kiểm tra giao dịch toàn vẹn (Transaction ACID) trên SQL Server: khi hoàn tất một hóa đơn, hệ thống phải vừa lưu hóa đơn, vừa chuyển trạng thái sản phẩm sang 'Đã bán', vừa cộng điểm tích lũy cho khách hàng trong cùng một giao dịch an toàn.

---

## 3. CÁC TỆP TIN Ở THƯ MỤC GỐC (ROOT)

* **`Class Diagram.xml`**: Bản vẽ thiết kế kiến trúc lớp (UML Class Diagram) chuẩn hóa của dự án. Mọi thực thể, thuộc tính và mối quan hệ trong mã nguồn đều phải khớp 100% với tệp này.
* **`.gitignore`**: Danh sách các tệp tạm thời sinh ra khi biên dịch (`bin/`, `obj/`) mà Git tự động bỏ qua, không đưa lên GitHub.
* **`BlinkBlinkJewelry.sln` & `BlinkBlinkJewelry.slnx`**: Tệp Solution tổng thể để mở và liên kết cả 6 dự án thành phần trên Visual Studio hoặc VS Code.
* **`README.md`**: Giới thiệu tổng quan về đồ án và danh sách thành viên thực hiện.
* **`WORKFLOW.md`**: Hướng dẫn quy trình sử dụng Git ngắn gọn cho nhóm 4 người làm việc song song không bị xung đột mã nguồn.
* **`CONG_NGHE_VA_QUY_TRINH.md`**: Bản mô tả công nghệ sử dụng và lộ trình thiết kế hệ thống từ trong ra ngoài (Domain-First).
