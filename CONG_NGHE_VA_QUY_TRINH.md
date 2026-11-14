# CÔNG NGHỆ SỬ DỤNG VÀ QUY TRÌNH THIẾT KẾ DỰ ÁN

Tài liệu này trình bày toàn bộ công nghệ được lựa chọn cho dự án Blink Blink Jewelry, mô hình kiến trúc C4 và quy trình từng bước xây dựng hệ thống từ cốt lõi ra bên ngoài theo đúng sơ đồ thiết kế kiến trúc lớp (Class Diagram), kiến trúc sạch (Clean Architecture) và hiện trạng mã nguồn thực tế của dự án.

---

## 1. CÔNG NGHỆ SỬ DỤNG

Hệ thống được xây dựng trên nền tảng .NET 8 (LTS) kết hợp kiến trúc Clean Architecture, tối ưu hóa độ ổn định, khả năng mở rộng và an toàn giao dịch:

| Thành phần | Công nghệ lựa chọn | Vai trò trong hệ thống |
| :--- | :--- | :--- |
| **Nền tảng & Ngôn ngữ** | C# trên nền .NET 8 (LTS) | Ngôn ngữ lập trình chính cho toàn bộ backend, thư viện nghiệp vụ và ứng dụng desktop; an toàn về kiểu dữ liệu và tối ưu hiệu năng dài hạn. |
| **Giao diện Quản trị (Desktop)** | Windows Forms (WinForms .NET 8) | Ứng dụng chạy trên máy trạm Windows dành cho **Admin**, **Nhân viên bán hàng (POS)** và **Thủ kho**. Hỗ trợ bán hàng tại quầy (POS), quản lý danh mục sản phẩm, theo dõi tồn kho theo thời gian thực, nhập kho và tiếp nhận bảo hành. Giao diện được thiết kế theo Taste Skill kim hoàn cao cấp: panel tinh tế, số liệu căn gióng chính xác với font tabular figures. |
| **Cổng Web Khách hàng (Web)** | ASP.NET Core 8 Web App (MVC + RESTful API) | Trang web mua sắm trực tuyến dành cho khách hàng tra cứu mẫu mã, giỏ hàng, đặt hàng và tra cứu bảo hành. Hệ thống kết hợp giao diện Web MVC (Razor Views, HTML5, CSS3, JavaScript) và cung cấp hệ thống RESTful Web API Controller phục vụ toàn diện các yêu cầu từ phía client. |
| **Hệ quản trị Cơ sở dữ liệu** | Microsoft SQL Server (`BlinkBlinkJewelryDb`) | Nơi lưu trữ tập trung toàn bộ dữ liệu kinh doanh (sản phẩm trang sức, hóa đơn, khách hàng, phiếu nhập kho, phiếu kiểm kê, bảo hành), bảo đảm chuẩn hóa quan hệ 3NF và hỗ trợ văn bản tiếng Việt Unicode có dấu. |
| **Tương tác CSDL & Tự động hóa** | ADO.NET thuần (`Microsoft.Data.SqlClient`) qua `DatabaseHelper` | Quản lý kết nối an toàn, thực thi câu lệnh SQL trực tiếp với tham số hóa 100% bằng `SqlParameter`, ngăn chặn triệt để SQL Injection. Tích hợp cơ chế **Tự động Khởi tạo CSDL (Auto-Database Provisioning)** và tự động tương thích lược đồ (`EnsureSchemaCompatibility`). |
| **Triển khai Cơ sở dữ liệu** | Kịch bản SQL (`BlinkBlinkJewelry.sql`) | Khởi tạo toàn bộ database `BlinkBlinkJewelryDb`, 15 bảng quan hệ và nạp sẵn dữ liệu mẫu thực tế của ngành kim hoàn. Được cấu hình nhúng (`EmbeddedResource`) trong thư viện `BlinkBlink.Infrastructure` để tự động tạo CSDL ngay khi khởi chạy chương trình. |
| **Tài liệu & Thử nghiệm API** | Swagger / OpenAPI (`Swashbuckle.AspNetCore 6.6.2`) | Tự động sinh tài liệu giao diện tương tác API tại địa chỉ `/swagger`, giúp kiểm thử và đối soát các endpoint dễ dàng. |
| **Chia sẻ tài nguyên liên nguồn** | CORS (`AllowAll`) | Cho phép Cổng Web khách hàng và các ứng dụng ngoại vi gọi API trực tiếp, an toàn và linh hoạt. |
| **Cơ chế quản lý phụ thuộc** | Dependency Injection (DI) | Cơ chế tiêm phụ thuộc có sẵn của .NET giúp kết nối các tầng với nhau thông qua hợp đồng (Interface), đảm bảo tính tách biệt và độc lập trong kiểm thử. |
| **Kiểm thử tự động** | xUnit & Moq | Bộ 35 ca kiểm thử tự động đạt tỷ lệ đạt 100% (gồm `BlinkBlink.UnitTests` kiểm tra logic Use Cases qua Mock Repositories và `BlinkBlink.IntegrationTests` kiểm tra kết nối CSDL và provisioning). |

---

## 2. MÔ HÌNH KIẾN TRÚC C4 TRONG THIẾT KẾ HỆ THỐNG

Theo yêu cầu chuẩn hóa, hệ thống được phân rã và tài liệu hóa theo **Mô hình C4 (Context, Container, Component, Code)**:

### 2.1. Mức 1 - Bối cảnh hệ thống (System Context)
Chỉ ra ranh giới giữa Hệ thống Blink Blink Jewelry và các đối tượng bên ngoài:
- **Admin, Nhân viên bán hàng & Thủ kho:** Tương tác trực tiếp trên máy trạm cửa hàng thông qua Ứng dụng Desktop Windows Forms.
- **Khách hàng:** Tương tác từ xa qua Cổng Web Khách hàng (trình duyệt web trên máy tính hoặc thiết bị di động), thực hiện xem bộ sưu tập trang sức, tìm kiếm, giỏ hàng, đặt hàng trực tuyến và tra cứu tiến độ bảo hành.

### 2.2. Mức 2 - Vỏ chứa (Container Diagram)
Phân rã hệ thống thành các khối ứng dụng và kho lưu trữ:
- **Desktop Admin App (`BlinkBlink.Admin` - WinForms .NET 8):** Chạy trên máy trạm Windows, phục vụ quản trị và bán hàng tại quầy (POS).
- **Web Storefront & Backend API (`BlinkBlink.Web` - ASP.NET Core 8 Web App):** Vừa phục vụ giao diện Web MVC Razor Views cho khách hàng, vừa cung cấp RESTful Web API endpoints cho toàn hệ sinh thái.
- **Thư viện nghiệp vụ dùng chung (Clean Architecture Libraries):**
  - `BlinkBlink.Domain`: Chứa các thực thể cốt lõi, Enums, Exceptions và hợp đồng giao tiếp (Repository Interfaces).
  - `BlinkBlink.Application`: Chứa các ca sử dụng (Use Cases), DTOs và bộ quy tắc kiểm tra tính hợp lệ.
  - `BlinkBlink.Infrastructure`: Chứa `DatabaseHelper`, các lớp hiện thực Repository bằng ADO.NET thuần và tài nguyên nhúng SQL.
- **Database (Microsoft SQL Server):** Lưu trữ dữ liệu tập trung với 15 bảng quan hệ chuẩn 3NF.

### 2.3. Mức 3 - Thành phần (Component Diagram)
Mô tả chi tiết cấu trúc bên trong của từng khối ứng dụng:

#### A. Bên trong Ứng dụng Desktop WinForms (`BlinkBlink.Admin`):
- **Khối Điều hướng & Xác thực:**
  - `LoginForm.cs`: Màn hình đăng nhập khởi động, xác thực người dùng qua `INguoiDungRepository`, kiểm tra trạng thái hoạt động và phân quyền theo vai trò (`ADMIN`, `SALE`, `WAREHOUSE`).
  - `MainForm.cs`: Vỏ điều hướng chính, header hiển thị thông tin tài khoản chuẩn định dạng `Tài khoản: {username} [{role}]`, tích hợp nút `🔑 Đổi Mật Khẩu` và `🚪 Đăng Xuất`.
  - `ChangePasswordDialog.cs`: Hộp thoại thay đổi mật khẩu người dùng.
- **Khối Bán hàng & Đơn hàng:**
  - `OrdersView.cs`: Quản lý đơn đặt hàng trực tuyến từ Web, tiếp nhận duyệt đơn, cập nhật trạng thái giao hàng hoặc hủy đơn.
  - `OfflineSalesView.cs`: Màn hình **POS Bán hàng tại quầy** chuyên dụng: tìm kiếm và chọn trang sức nhanh, chọn hoặc thêm khách hàng theo số điện thoại, áp dụng chiết khấu/giảm giá, lựa chọn phương thức thanh toán (Tiền mặt hoặc Chuyển khoản QR code ngân hàng), tạo hóa đơn và in phiếu thanh toán.
- **Khối Sản phẩm & Kho hàng:**
  - `ProductsView.cs`: Màn hình quản lý danh mục trang sức, bộ lọc đa tiêu chí (danh mục, chất liệu, size, trạng thái). Cột **TỒN KHO** được tính toán tự động `Tổng nhập (ChiTietNhapKho) - Tổng xuất (ChiTietHoaDon không bị hủy)`, hiển thị định dạng số tabular với màu ngọc lục bảo (còn hàng) và màu đỏ (hết hàng).
  - `ProductEditDialog.cs`: Hộp thoại thêm mới hoặc cập nhật thông tin trang sức, quản lý hình ảnh và giá niêm yết.
  - `CategoryView.cs`: Quản lý danh mục phân loại trang sức.
  - `ProductsAndInventoryContainerView.cs`: Khối điều hướng gom nhóm tab giữa sản phẩm và kho hàng.
  - `InventoryInwardView.cs`: Lập và xác nhận phiếu nhập kho từ nhà cung cấp hoặc xưởng chế tác.
- **Khối Dịch vụ & Bảo hành:**
  - `WarrantyView.cs`: Tiếp nhận sửa chữa, đánh bóng trang sức, ghi nhận chi phí bảo hành ước tính / thực tế, theo dõi tiến độ và bàn giao sản phẩm.
- **Khối Người dùng & Phân quyền:**
  - `UsersView.cs`: Quản lý danh sách nhân viên và khách hàng, thống kê KPI số lượng Quản trị viên và nhân sự theo vai trò.
- **Khối Báo cáo & Thống kê:**
  - `ReportingView.cs`: Biểu đồ và bảng thống kê doanh thu theo mốc thời gian, tỷ trọng danh mục bán chạy và cảnh báo tồn kho.

#### B. Bên trong Cổng Web & API (`BlinkBlink.Web`):
- **Khối Giao diện Web MVC:**
  - `HomeController.cs`: Điều phối trang chủ (`Index.cshtml`), trang tìm kiếm và bộ lọc trang sức (`TimKiem.cshtml`), trang điều khoản bảo mật (`Privacy.cshtml`).
  - Hệ thống giao diện tĩnh trong `wwwroot/`: CSS responsive chuẩn kim hoàn, JavaScript xử lý giỏ hàng động và tương tác bộ sưu tập.
- **Khối RESTful API Controllers (`[ApiController]`):**
  - `SanPhamController.cs`: API tra cứu danh mục, danh sách sản phẩm trang sức, lọc theo chất liệu, size, giá bán.
  - `BanHangController.cs`: API xử lý đặt hàng trực tuyến, thanh toán và tra cứu hóa đơn.
  - `GioHangController.cs`: API quản lý giỏ hàng (thêm món, bớt món, cập nhật số lượng, xóa giỏ hàng).
  - `BaoHanhController.cs`: API tra cứu tiến độ xử lý phiếu bảo hành theo số điện thoại hoặc mã phiếu.
  - `KhachHangController.cs`: API quản lý thông tin tài khoản và lịch sử giao dịch của khách hàng.
  - `XacThucController.cs`: API đăng nhập, đổi mật khẩu và cấp quyền người dùng.
  - `PhieuNhapKhoController.cs`: API truy xuất dữ liệu nhập kho trang sức.
  - `ThongKeController.cs`: API cung cấp dữ liệu báo cáo doanh thu và tồn kho.

#### C. Bên trong Tầng Ứng dụng (`BlinkBlink.Application`):
Các Ca sử dụng độc lập tuân thủ nguyên lý Đơn trách nhiệm (Single Responsibility Principle):
- `DangNhapUseCase.cs`, `DangKyKhachHangUseCase.cs`, `QuanLyNhanVienUseCase.cs`, `QuanLyKhachHangUseCase.cs`.
- `QuanLySanPhamUseCase.cs`.
- `DatHangUseCase.cs`, `XuLyDonHangUseCase.cs`, `BanHangOfflineUseCase.cs`.
- `XacNhanNhapKhoUseCase.cs`.
- `TiepNhanBaoHanhUseCase.cs`, `XuLyBaoHanhUseCase.cs`.
- `ThongKeDoanhThuUseCase.cs`, `ThongKeKhoHangUseCase.cs`.

### 2.4. Mức 4 - Mã nguồn (Code / Class Diagram)
Hiện thực hóa chính xác 100% cấu trúc các lớp đối tượng, các thuộc tính, phương thức và mối quan hệ đã được thiết kế trong sơ đồ kiến trúc lớp:
- **Thực thể Lõi (`BlinkBlink.Domain/Entities/`):**
  - `Identity/`: `NguoiDung.cs` (lớp trừu tượng định nghĩa các nghiệp vụ cốt lõi: `DangNhap`, `DoiMatKhau`, `CapNhatThongTin`, `KhoaTaiKhoan`), `NhanVien.cs` (kế thừa `NguoiDung`), `VaiTro.cs`, `AuditLog.cs`.
  - `Catalog/`: `SanPhamTrangSuc.cs` (chứa các thuộc tính `MaSanPham`, `TenSanPham`, `MaDanhMuc`, `ChatLieu`, `Size`, `TrangThai`, `GiaNhap`, `GiaBan`, `HinhAnh`, `PhanTramGiamGia`, `SoLuongTon`. Theo quy chuẩn nghiệp vụ, chất liệu lưu chuỗi string, không quản lý lịch sử giá vàng hay biến động thị trường vàng), `DanhMuc.cs`, `LichSuGia.cs`.
  - `Orders/`: `HoaDon.cs`, `ChiTietHoaDon.cs`.
  - `Cart/`: `GioHang.cs`, `ChiTietGioHang.cs`.
  - `Customers/`: `KhachHang.cs` (kế thừa `NguoiDung`).
  - `Inventory/`: `PhieuNhapKho.cs`, `ChiTietNhapKho.cs`, `PhieuKiemKe.cs`, `ChiTietKiemKe.cs`.
  - `Warranty/`: `PhieuBaoHanh.cs`.
- **Hợp đồng Dữ liệu (`BlinkBlink.Domain/Interfaces/`):**
  - 12 Interfaces Repository: `INguoiDungRepository`, `INhanVienRepository`, `IKhachHangRepository`, `IVaiTroRepository`, `ISanPhamRepository`, `IDanhMucRepository`, `IHoaDonRepository`, `IGioHangRepository`, `IPhieuNhapKhoRepository`, `IPhieuKiemKeRepository`, `IBaoHanhRepository`, `IThongKeRepository`.

---

## 3. QUY TRÌNH THIẾT KẾ: TỪ ĐÂU ĐẾN ĐÂU? (DOMAIN-FIRST / INSIDE-OUT)

Hệ thống được phát triển theo phương pháp luận **Thiết kế hướng đối tượng từ trong ra ngoài (Domain-First / Inside-Out)** theo chuẩn Clean Architecture gồm 5 giai đoạn chặt chẽ:

```
[Giai đoạn 1: Tầng Lõi Nghiệp Vụ]       Xây dựng Entities & Quy tắc nghiệp vụ từ Sơ đồ Kiến trúc Lớp (Class Diagram)
               │
               ▼
[Giai đoạn 2: Tầng Cơ Sở Dữ Liệu]       Thiết kế Script SQL Server (15 bảng 3NF) & Lập trình DatabaseHelper ADO.NET
               │
               ▼
[Giai đoạn 3: Tầng Ứng Dụng]            Hiện thực hóa các Ca Sử Dụng (Use Cases) độc lập điều phối quy trình
               │
               ▼
[Giai đoạn 4: Tầng Giao Diện]           Ghép WinForms Desktop POS & Cổng Web Khách hàng (MVC + RESTful API)
               │
               ▼
[Giai đoạn 5: Tầng Kiểm Thử Tự Động]    Xây dựng bài test xUnit (Unit Tests & Integration Tests) bảo đảm an toàn 100%
```

---

#### Giai đoạn 1: Xây dựng Tầng Lõi Nghiệp vụ (`BlinkBlink.Domain`)
Hiện thực hóa các thực thể nghiệp vụ hoàn toàn độc lập với công nghệ lưu trữ và giao diện:
1. **Lớp trừu tượng cơ sở (`NguoiDung.cs`):** Định nghĩa thuộc tính chung của tài khoản (`MaNguoiDung`, `HoTen`, `SoDienThoai`, `TenDangNhap`, `MatKhau`, `MaVaiTro`, `TrangThai`) và các hành vi tự thân (`DangNhap`, `DoiMatKhau`, `CapNhatThongTin`, `KhoaTaiKhoan`).
2. **Các thực thể nghiệp vụ kế thừa và liên kết:**
   - `Identity/`: `VaiTro.cs`, `NhanVien.cs` (kế thừa `NguoiDung`), `AuditLog.cs`.
   - `Catalog/`: `DanhMuc.cs`, `SanPhamTrangSuc.cs` (đầy đủ thuộc tính kích thước, chất liệu lưu dạng chuỗi string, tỷ lệ giảm giá, hình ảnh và số lượng tồn).
   - `Orders/`: `HoaDon.cs`, `ChiTietHoaDon.cs` (tính tiền hàng, giảm giá, tổng thanh toán).
   - `Cart/`: `GioHang.cs`, `ChiTietGioHang.cs` (quản lý giỏ hàng trực tuyến).
   - `Customers/`: `KhachHang.cs` (kế thừa `NguoiDung`, quản lý khách hàng thân thiết).
   - `Inventory/`: `PhieuNhapKho.cs`, `ChiTietNhapKho.cs`, `PhieuKiemKe.cs`, `ChiTietKiemKe.cs` (kiểm soát luân chuyển hàng tồn).
   - `Warranty/`: `PhieuBaoHanh.cs` (tiếp nhận sửa chữa, đánh bóng và chi phí dịch vụ).
3. **Các hợp đồng giao tiếp (Interfaces):** Định nghĩa 12 interface Repository làm hợp đồng ràng buộc cho tầng hạ tầng dữ liệu.

---

#### Giai đoạn 2: Hiện thực hóa Cơ sở dữ liệu & Truy xuất Dữ liệu (`BlinkBlink.Infrastructure`)
Triển khai cơ sở dữ liệu Microsoft SQL Server và tầng truy xuất dữ liệu bằng ADO.NET thuần:
1. **Kịch bản cơ sở dữ liệu `BlinkBlinkJewelry.sql`:**
   - Khởi tạo database `BlinkBlinkJewelryDb`, 15 bảng quan hệ, khóa chính, khóa ngoại, chỉ mục và các ràng buộc toàn vẹn dữ liệu.
   - Nạp sẵn dữ liệu mẫu thực tế của ngành kim hoàn (vai trò, nhân viên, khách hàng, 4 danh mục trang sức, sản phẩm độc bản, hóa đơn, phiếu nhập kho, phiếu bảo hành).
   - Tài khoản quản trị mặc định: `admin` / `123456789`.
2. **Lớp tiện ích truy xuất dữ liệu `DatabaseHelper.cs`:**
   - Quản lý vòng đời kết nối `SqlConnection` với cơ chế tham số hóa `SqlParameter` tuyệt đối, an toàn chống tấn công SQL Injection.
   - Cung cấp các hàm thực thi đa năng: `ExecuteNonQuery`, `ExecuteReader`, `ExecuteScalar`, `ExecuteDataTable`.
   - **Cơ chế Tự động Khởi tạo CSDL (Auto-Database Provisioning):** Phương thức `TryProvisionDatabase` tự động dò tìm các SQL Server instances cục bộ (`.`, `localhost`, `.\MSSQL`, `(localdb)\mssqllocaldb`), kết nối qua database `master`, tự tạo `BlinkBlinkJewelryDb`, phân tách khối lệnh `GO` và chạy toàn bộ script DDL/DML từ tài nguyên nhúng `EmbeddedResource` trong DLL hoặc file vật lý.
   - Cơ chế tự kiểm tra tương thích lược đồ `EnsureSchemaCompatibility()`.
3. **12 Repositories ADO.NET thuần:**
   - Triển khai cụ thể toàn bộ 12 Repositories sử dụng `SqlCommand` và `SqlDataReader`, loại bỏ hoàn toàn sự phụ thuộc vào Entity Framework Core Migrations, giúp ứng dụng chạy ổn định và hiệu năng cao.

---

#### Giai đoạn 3: Hiện thực hóa Ca Sử Dụng Nghiệp Vụ (`BlinkBlink.Application`)
Đóng vai trò người điều phối nghiệp vụ (Use Cases), tiếp nhận DTO từ tầng Presentation, thực thi logic và tương tác với tầng Domain và Repository:
1. **Phân hệ Người dùng & Phân quyền:**
   - `DangNhapUseCase`: Xác thực tài khoản người dùng, kiểm tra trạng thái hoạt động.
   - `DangKyKhachHangUseCase`: Tạo tài khoản khách hàng mới, kiểm tra trùng lặp thông tin.
   - `QuanLyNhanVienUseCase`: Thêm mới nhân viên, cập nhật mức lương cơ bản, phân vai trò, kiểm tra ràng buộc chứng từ trước khi xóa.
   - `QuanLyKhachHangUseCase`: Quản lý thông tin khách hàng, tra cứu lịch sử mua sắm.
2. **Phân hệ Bán hàng & Đơn hàng:**
   - `BanHangOfflineUseCase`: Phục vụ POS bán hàng tại quầy: xử lý lập hóa đơn bán lẻ, trừ kho tức thời, cập nhật thông tin khách hàng qua số điện thoại.
   - `DatHangUseCase`: Tiếp nhận đơn đặt hàng trực tuyến từ Web, trừ kho sản phẩm.
   - `XuLyDonHangUseCase`: Duyệt đơn, cập nhật trạng thái giao hàng hoặc hủy đơn.
3. **Phân hệ Quản lý Sản phẩm & Tồn kho:**
   - `QuanLySanPhamUseCase`: Thêm mới, cập nhật giá bán niêm yết, thông tin sản phẩm và phân loại.
   - `XacNhanNhapKhoUseCase`: Lập phiếu nhập kho từ nhà chế tác, cập nhật số lượng tồn kho.
4. **Phân hệ Bảo hành & Dịch vụ:**
   - `TiepNhanBaoHanhUseCase`: Lập phiếu tiếp nhận bảo hành trang sức, ghi nhận tình trạng sản phẩm và chi phí dự tính.
   - `XuLyBaoHanhUseCase`: Cập nhật tiến độ xử lý, chi phí thực tế và ngày bàn giao.
5. **Phân hệ Báo cáo & Thống kê:**
   - `ThongKeDoanhThuUseCase`: Tổng hợp doanh số bán hàng theo ngày, tháng, năm.
   - `ThongKeKhoHangUseCase`: Báo cáo hàng tồn, chênh lệch kiểm kê và danh sách sản phẩm bán chạy.

---

#### Giai đoạn 4: Xây dựng Giao diện Người dùng (Presentation)
1. **Ứng dụng Desktop Quản trị WinForms (`BlinkBlink.Admin`):**
   - Dành cho **Admin**, **Nhân viên bán hàng (POS)** và **Thủ kho**.
   - `MainForm.cs`: Vỏ điều hướng trung tâm chuyển đổi các phân hệ trên `panelContent`.
   - Vòng lặp phiên làm việc tuần tự giữa `LoginForm` và `MainForm`, đảm bảo người dùng có thể đăng nhập, đăng xuất và đổi mật khẩu mượt mà.
   - Các UserControls chuyên biệt: POS Bán hàng tại quầy (`OfflineSalesView`), Quản lý đơn hàng (`OrdersView`), Danh mục sản phẩm (`ProductsView`, `ProductEditDialog`, `CategoryView`), Kho hàng (`InventoryInwardView`), Bảo hành dịch vụ (`WarrantyView`), Quản lý người dùng (`UsersView`), Báo cáo doanh thu (`ReportingView`).
2. **Cổng Web Khách hàng (`BlinkBlink.Web`):**
   - Vừa là Web App phục vụ mua sắm trực tuyến bằng ASP.NET Core MVC (Razor Views).
   - Vừa là hệ thống RESTful Web API Controller với tài liệu Swagger UI trực quan tại `/swagger`.
   - Hỗ trợ chính sách CORS (`AllowAll`) cho phép mở rộng kết nối với các ứng dụng di động hoặc đối tác bên ngoài.

---

#### Giai đoạn 5: Kiểm thử Tự động & Đảm bảo Chất lượng (`tests/`)
1. **Kiểm thử đơn vị (`BlinkBlink.UnitTests`):**
   - Sử dụng xUnit kết hợp Moq giả lập dữ liệu độc lập cho toàn bộ 5 phân hệ dịch vụ (`Catalog`, `Identity`, `Orders`, `Reporting`, `Warranty`).
   - Kiểm tra tính toán tổng tiền đơn hàng, chiết khấu, ràng buộc chứng từ khi xóa nhân viên/khách hàng, và quy tắc nghiệp vụ bảo hành.
2. **Kiểm thử tích hợp (`BlinkBlink.IntegrationTests`):**
   - `DatabaseHelperTests.cs`: Kiểm tra tính toàn vẹn kết nối CSDL và cơ chế tự động khởi tạo database (`TryProvisionDatabase`).
3. **Kết quả nghiệm thu:** Toàn bộ **35/35 test cases PASS 100%**, biên dịch 0 error, 0 warning trên nền tảng .NET 8 LTS.

---

## 4. QUY CHUẨN THIẾT KẾ GIAO DIỆN (TASTE SKILL GUIDELINES)

Cả hai giao diện Desktop và Web đều tuân thủ các quy tắc thẩm mỹ cao cấp dành riêng cho ngành hàng trang sức kim hoàn:

### 4.1. Bảng màu & Phong cách Kim hoàn Cao cấp
- **Màu sắc chủ đạo:**
  - Xanh ngọc lục bảo (Emerald Green `#0F5132`, `#198754`): Tượng trưng cho sự sang trọng, tinh tế và tin cậy.
  - Vàng kim quý phái (Gold Metallic `#D4AF37`, `#B8860B`): Tạo điểm nhấn quý phái cho giá niêm yết, chứng nhận chất lượng và nút hành động chính.
  - Nền phụ trợ thanh lịch: Nền trắng ngà và xám sáng (`#F8F9FA`, `#FFFFFF`) làm nổi bật chi tiết sản phẩm.
- **Quy tắc Anti-Slop (Chống hình ảnh rác):**
  - Tuyệt đối không dùng icon giả, placeholder méo mó. Mọi sản phẩm trang sức hiển thị đều sử dụng hình ảnh thật hoặc tài nguyên chuẩn mực của dự án.

### 4.2. Chuẩn mực Giao diện WinForms Desktop POS
- **Căn gióng số liệu chính xác (Tabular Figures):** Toàn bộ các cột tiền tệ (giá bán, tổng thanh toán, chi phí sửa chữa) và số lượng tồn kho trên DataGridView đều sử dụng font số có độ rộng ký tự đồng đều (tabular numbers), căn lề phải rõ ràng.
- **Trạng thái màu sắc theo dữ liệu thực tế:** Cột tồn kho tự động đổi màu phản ánh tình trạng hàng hóa (màu xanh ngọc khi còn hàng trong kho, màu đỏ cảnh báo khi tồn kho bằng 0).
- **Trải nghiệm bán hàng tại quầy (POS):** Bố cục chia rõ ràng giữa danh mục tìm kiếm sản phẩm bên trái và giỏ hàng tính tiền bên phải, hỗ trợ quét mã/chọn nhanh, điền thông tin khách hàng qua số điện thoại và tích hợp mã QR chuyển khoản tiện lợi.

---

## 5. QUY TRÌNH LÀM VIỆC NHÓM & QUẢN LÝ TIẾN ĐỘ

### 5.1. Quy trình Phối hợp qua Git (Git Workflow)
Để đảm bảo không xung đột mã nguồn và kiểm soát chất lượng chặt chẽ:
1. **Quy tắc bất biến:**
   - Tuyệt đối không viết code và không đẩy code trực tiếp lên nhánh `main`.
   - Cấm sử dụng lệnh `git push -f` gây ghi đè lịch sử của nhóm.
2. **Quy ước đặt tên nhánh:**
   - `feature/<tên tính năng>`: Nhánh phát triển tính năng mới (ví dụ: `feature/identity-management-module`, `feature/cart-orders-foundation-module`).
   - `bugfix/<lỗi cần sửa>`: Nhánh khắc phục sự cố kỹ thuật.
   - `refactor/<tên module>`: Nhánh tối ưu hóa cấu trúc mã nguồn.
3. **Các bước phát triển một tính năng:**
   ```bash
   # Bước 1: Kéo code mới nhất từ nhánh chính
   git checkout main
   git pull origin main

   # Bước 2: Tạo nhánh riêng theo tính năng
   git checkout -b feature/ten-tinh-nang

   # Bước 3: Lập trình, kiểm tra biên dịch và chạy test
   dotnet build
   dotnet test

   # Bước 4: Lưu thay đổi cục bộ
   git status
   git add .
   git commit -m "Mo ta ro rang cong viec da hoan thanh"

   # Bước 5: Đẩy nhánh lên kho chứa từ xa
   git push origin feature/ten-tinh-nang

   # Bước 6: Mở Pull Request (PR) trên GitHub để thành viên khác đánh giá (Review Code) trước khi gộp vào main
   ```

### 5.2. Chu trình Quản lý Mục tiêu & Tiến độ Dự án
Tiến độ công việc được quản lý nhất quán thông qua bộ ba tệp tài liệu:
- **`TODO.md`:** Quản lý các mục tiêu ngắn hạn đang thực hiện. Mọi tính năng hoặc yêu cầu mới đều được ghi nhận vào danh sách `- [ ]` trước khi thực thi.
- **`COMPLETED.md`:** Lưu vết toàn bộ các đầu việc đã hoàn thành và nghiệm thu thành công theo định dạng:
  ```markdown
  - [x] [YYYY-MM-DD] [Tên Task]: Tóm tắt ngắn gọn các tệp mã nguồn đã tạo/sửa và chức năng hoàn tất.
  ```
- **`SCRATCHPAD.md`:** Bộ nhớ tạm ghi nhớ trạng thái công việc dở dang giữa các phiên làm việc, đảm bảo tính liên tục của dự án (gồm: các hạng mục đã hoàn thành, quyết định kỹ thuật đã chốt, và hành động cụ thể kế tiếp).
