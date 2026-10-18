# CÔNG NGHỆ SỬ DỤNG VÀ QUY TRÌNH THIẾT KẾ DỰ ÁN

Tài liệu này trình bày toàn bộ công nghệ được lựa chọn cho dự án Blink Blink Jewelry, mô hình kiến trúc C4 và quy trình từng bước xây dựng hệ thống từ cốt lõi ra bên ngoài theo đúng bản vẽ `Class Diagram.xml`.

---

## 1. CÔNG NGHỆ SỬ DỤNG

Hệ thống được xây dựng trên một nền tảng thống nhất của Microsoft để đảm bảo tính đồng bộ, hiệu năng cao và dễ bảo trì:

| Thành phần | Công nghệ lựa chọn | Vai trò trong hệ thống |
| :--- | :--- | :--- |
| **Nền tảng & Ngôn ngữ** | C# trên nền .NET 8 (LTS) | Ngôn ngữ lập trình chính cho toàn bộ hệ thống, an toàn về kiểu dữ liệu và tối ưu hiệu năng dài hạn. |
| **Giao diện Quản trị (Desktop)** | WPF (.NET 8) với **UserControl** | Ứng dụng chạy trên máy tính Windows tại quầy giao dịch, dành cho Quản lý, Thu ngân và Thủ kho. Áp dụng mô hình MVVM (Model-View-ViewModel) kết hợp kiến trúc UserControl giúp từng màn hình nghiệp vụ độc lập, dễ thay thế và tái sử dụng. |
| **Giao diện Khách hàng (Web)** | ASP.NET Core MVC (.NET 8) | Trang web chạy trên trình duyệt internet dành cho khách hàng tra cứu mẫu mã, giỏ hàng, thông tin thẻ thành viên và phiếu bảo hành; dùng chung tầng nghiệp vụ với ứng dụng Desktop. |
| **Hệ quản trị Cơ sở dữ liệu** | Microsoft SQL Server | Nơi lưu trữ toàn bộ dữ liệu kinh doanh của cửa hàng (sản phẩm, hóa đơn, khách hàng, phiếu nhập kho, phiếu kiểm kê, bảo hành, nhật ký kiểm toán), bảo đảm chuẩn hóa quan hệ 3NF. |
| **Tương tác CSDL (ORM)** | Entity Framework Core 8 | Tự động ánh xạ 18 lớp đối tượng C# thành 18 bảng trong SQL Server; tự động sinh và cập nhật cấu trúc database bằng công cụ Migrations. |
| **Cơ chế quản lý phụ thuộc** | Dependency Injection (DI) | Cơ chế tiêm phụ thuộc có sẵn của .NET giúp kết nối các tầng với nhau thông qua hợp đồng (Interface), đảm bảo tính linh hoạt và dễ kiểm thử. |
| **Kiểm thử tự động** | xUnit | Công cụ viết mã kiểm thử tự động xem logic tính tiền hóa đơn, xét hạng thẻ thành viên và giao dịch CSDL có chạy chính xác hay không. |

---

## 2. MÔ HÌNH KIẾN TRÚC C4 TRONG THIẾT KẾ HỆ THỐNG

Theo yêu cầu của giảng viên, hệ thống được phân rã và tài liệu hóa theo **Mô hình C4 (Context, Container, Component, Code)**:

1. **Mức 1 - Bối cảnh hệ thống (System Context):**
   - Chỉ ra ranh giới giữa Hệ thống Blink Blink Jewelry và các đối tượng bên ngoài:
     - **Quản lý / Thu ngân / Thủ kho:** Tương tác trực tiếp qua Ứng dụng Desktop WPF.
     - **Khách hàng:** Tương tác từ xa qua Cổng thông tin Web ASP.NET Core MVC.
2. **Mức 2 - Vỏ chứa (Container Diagram):**
   - Phân rã hệ thống thành các khối ứng dụng chạy độc lập:
     - **Desktop Admin App (WPF .NET 8):** Chạy trên máy trạm của cửa hàng.
     - **Customer Web App (ASP.NET Core MVC .NET 8):** Chạy trên máy chủ web.
     - **Shared Core Libraries (`Domain`, `Application`, `Infrastructure`):** Khối thư viện logic dùng chung.
     - **Database (SQL Server):** Lưu trữ dữ liệu tập trung.
3. **Mức 3 - Thành phần (Component Diagram):**
   - Bên trong ứng dụng Desktop: Gồm các **UserControl** nghiệp vụ (`UC_BanHangPOS`, `UC_SanPham`, `UC_NhapKho`, `UC_KiemKe`, `UC_KhachHang`, `UC_BaoHanh`, `UC_NhanVien`, `UC_AuditLog`) kết nối với các ViewModel và Service tương ứng.
   - Bên trong ứng dụng Web: Gồm các Controller (`HomeController`, `ProductsController`, `CartController`, `WarrantyController`, `AccountController`).
   - Bên trong tầng Application: Gồm 7 nhóm dịch vụ nghiệp vụ độc lập theo nguyên lý đơn trách nhiệm (Single Responsibility Principle).
4. **Mức 4 - Mã nguồn (Code / Class Diagram):**
   - Thể hiện chi tiết cấu trúc 18 lớp đối tượng, các thuộc tính, phương thức và mối quan hệ (1-n, kế thừa, hiện thực hóa interface) đã được định nghĩa chuẩn xác trong tệp `Class Diagram.xml`.

---

## 3. QUY TRÌNH THIẾT KẾ: TỪ ĐÂU ĐẾN ĐÂU?

Hệ thống được xây dựng theo phương pháp luận **Thiết kế hướng đối tượng từ trong ra ngoài (Domain-First / Inside-Out)** theo chuẩn kiến trúc sạch (Clean Architecture):

### Vì sao không bắt đầu từ giao diện hay viết câu lệnh SQL trước?
- Nếu làm giao diện trước: Giao diện sửa đến đâu thì code bị phá hỏng đến đó, dữ liệu bị trói chặt vào các nút bấm màn hình.
- Nếu viết SQL trước: Dễ sa vào tư duy lập trình thủ tục, biến các đối tượng C# thành những cái "vỏ rỗng" chỉ biết hứng dữ liệu (Anemic Domain Model).
- **Quy tắc đúng:** Nghiệp vụ kim hoàn và bản vẽ `Class Diagram.xml` là thứ cốt lõi nhất. Khi đã xây dựng đúng các đối tượng nghiệp vụ, cơ sở dữ liệu và giao diện sẽ tự nhiên được hình thành xoay quanh cái lõi đó.

---

### Quy trình 5 giai đoạn phát triển tuần tự:

```
[Giai đoạn 1: Tầng Lõi]        Xây dựng 18 Thực thể (Entities) & Quy tắc nghiệp vụ từ Class Diagram
           │
           ▼
[Giai đoạn 2: Tầng Cơ sở dữ liệu] Cấu hình 18 bảng & Chạy Migration sinh database trên SQL Server
           │
           ▼
[Giai đoạn 3: Tầng Dịch vụ]     Hiện thực hóa 7 nhóm Dịch vụ nghiệp vụ (Bán hàng, Kho, Khách hàng,...)
           │
           ▼
[Giai đoạn 4: Tầng Giao diện]   Ghép UserControl vào Desktop WPF (Admin) và Razor Views vào Web (Khách hàng)
           │
           ▼
[Giai đoạn 5: Tầng Kiểm thử]    Viết mã kiểm thử tự động để bảo đảm tính đúng đắn và an toàn giao dịch
```

---

#### Giai đoạn 1: Xây dựng Tầng Lõi Nghiệp vụ (`BlinkBlink.Domain`)
Hiện thực hóa chính xác 18 lớp từ `Class Diagram.xml` thành mã C# trong 7 thư mục nghiệp vụ:
1. **Lớp cơ sở (`BaseEntity.cs`):** Định nghĩa thuộc tính chung của mọi đối tượng (Mã định danh `Id`, Ngày tạo `NgayTao`).
2. **7 Phân hệ thực thể:**
   - `Identity/`: `VaiTro`, `NguoiDung` (interface), `NhanVien`, `AuditLog`.
   - `Catalog/`: `DanhMuc`, `SanPhamTrangSuc` (mã ID độc bản, chất liệu, size, giá nhập, giá bán), `LichSuGia` (lưu biến động giá nhập/bán).
   - `Orders/`: `HoaDon`, `ChiTietHoaDon` (tính tiền hàng, giảm giá thành viên, tổng thanh toán).
   - `Cart/`: `GioHang`, `ChiTietGioHang` (phục vụ khách mua sắm online trên Web).
   - `Customers/`: `KhachHang`, `TheThanhVien` (4 hạng thẻ VIP và phần trăm giảm giá tiền công).
   - `Inventory/`: `PhieuNhapKho`, `ChiTietNhapKho`, `PhieuKiemKe`, `ChiTietKiemKe` (đối soát tồn sổ sách vs tồn thực tế).
   - `Warranty/`: `PhieuBaoHanh` (tiếp nhận sửa chữa, đánh bóng, theo dõi tiến độ).
3. **Phương thức tự bảo vệ dữ liệu:**
   - Mỗi thực thể tự đóng gói các quy tắc nghiệp vụ của mình, ngăn chặn việc gán dữ liệu sai từ bên ngoài.

---

#### Giai đoạn 2: Hiện thực hóa Cơ sở dữ liệu (`BlinkBlink.Infrastructure`)
Chuyển hóa 18 lớp đối tượng thành các bảng quan hệ trong CSDL SQL Server:
1. **Lớp ngữ cảnh `AppDbContext`**: Kế thừa `DbContext` của EF Core, khai báo các thuộc tính `DbSet<T>`.
2. **Cấu hình Fluent API độc lập**: 
   - Tách từng file cấu hình trong thư mục `Data/Configurations/` kế thừa `IEntityTypeConfiguration<T>`.
   - Trong `AppDbContext.cs`, sử dụng `modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly)` để tự động quét nạp cấu hình, tránh 100% nguy cơ xung đột (conflict) mã nguồn giữa các thành viên.
3. **Cấu hình chuỗi kết nối (Connection String) trong `src/BlinkBlink.Web/appsettings.json`**:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BlinkBlinkJewelryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```
   *(Sử dụng SQL Server LocalDB mặc định có sẵn của Visual Studio, không cần cài đặt thêm phần mềm máy chủ).*

4. **Bộ câu lệnh chi tiết Migration & Quản lý CSDL (Entity Framework Core Tools)**:

   * **Bước 0: Cài đặt công cụ CLI (Chỉ cần chạy 1 lần duy nhất trên máy nếu chưa có):**
     ```bash
     dotnet tool install --global dotnet-ef
     ```
     *(Hoặc cập nhật nếu đã cài bản cũ: `dotnet tool update --global dotnet-ef`)*

   * **Bước 1: Tạo Migration (Sinh bản thiết kế thay đổi CSDL khi có sửa đổi Entity/Fluent API):**
     * Dùng .NET CLI tại thư mục gốc solution:
       ```bash
       dotnet ef migrations add <TenMigration> --project src/BlinkBlink.Infrastructure --startup-project src/BlinkBlink.Web
       ```
       *Ví dụ tạo bản CSDL đầu tiên:*
       ```bash
       dotnet ef migrations add InitialCreate --project src/BlinkBlink.Infrastructure --startup-project src/BlinkBlink.Web
       ```
     * Hoặc dùng Package Manager Console (PMC) trong Visual Studio (chọn Default project là `BlinkBlink.Infrastructure`):
       ```powershell
       Add-Migration <TenMigration>
       ```

   * **Bước 2: Cập nhật CSDL (Bắn lệnh SQL tự động tạo/sửa bảng trong SQL Server):**
     * Dùng .NET CLI:
       ```bash
       dotnet ef database update --project src/BlinkBlink.Infrastructure --startup-project src/BlinkBlink.Web
       ```
     * Hoặc dùng Package Manager Console (PMC):
       ```powershell
       Update-Database
       ```

   * **Quy trình phối hợp làm việc nhóm khi kéo code từ Git:**
     Khi bạn khác trong nhóm vừa thêm bảng mới và merge vào `main`, các thành viên còn lại chỉ cần chạy 2 lệnh:
     ```bash
     git pull origin main
     dotnet ef database update --project src/BlinkBlink.Infrastructure --startup-project src/BlinkBlink.Web
     ```
     *(CSDL SQL Server trên máy bạn sẽ tự động cập nhật khớp 100% với nhóm mà không cần gửi file .bak hay .sql qua lại).*

   * **Câu lệnh hoàn tác / Rollback khi lỡ tạo sai Migration:**
     * Trả CSDL về phiên bản Migration trước:
       ```bash
       dotnet ef database update <TenMigrationTruocDo> --project src/BlinkBlink.Infrastructure --startup-project src/BlinkBlink.Web
       ```
     * Xóa file Migration vừa tạo (khi chưa apply hoặc đã rollback):
       ```bash
       dotnet ef migrations remove --project src/BlinkBlink.Infrastructure --startup-project src/BlinkBlink.Web
       ```

---

#### Giai đoạn 3: Hiện thực hóa Ca Sử Dụng & Dịch Vụ Nghiệp Vụ (`BlinkBlink.Application`)
Đóng vai trò người điều phối, tiếp nhận yêu cầu từ màn hình và gọi tầng Domain/Infrastructure xử lý:
1. **Dịch vụ Bán hàng (`OrderService`):** Nhận mã sản phẩm tại quầy POS, tính tiền chiết khấu thành viên, lưu hóa đơn, cập nhật trạng thái sản phẩm sang 'Đã bán' trong một giao dịch an toàn (Transaction ACID).
2. **Dịch vụ Sản phẩm & Giá (`ProductService`, `PriceHistoryService`):** Cập nhật giá bán/nhập mới và tự động ghi log vào bảng `LichSuGia`.
3. **Dịch vụ Kho hàng (`StockInService`, `StockCheckService`):** Lập phiếu nhập kho và biên bản kiểm kê, tính toán lượng chênh lệch thực tế vs sổ sách.
4. **Dịch vụ Khách hàng & Thành viên (`CustomerService`):** Sau mỗi hóa đơn thành công, tự động tích lũy điểm và thăng hạng thẻ VIP khi đạt mốc.
5. **Dịch vụ Bảo hành (`WarrantyService`):** Lập phiếu bảo hành, cập nhật tiến độ xử lý và chi phí sửa chữa.
6. **Dịch vụ Xác thực & Kiểm toán (`AuthService`, `AuditLogService`):** Xác thực tài khoản và lưu vết thao tác người dùng.

---

#### Giai đoạn 4: Xây dựng Giao diện Người dùng
1. **Ứng dụng Desktop Quản trị WPF (`BlinkBlink.Admin`):**
   - Thiết kế `MainWindow.xaml` với menu điều hướng chính.
   - Xây dựng các **UserControl** độc lập nhúng vào màn hình chính theo yêu cầu của giảng viên:
     - `UC_BanHangPOS.xaml`: Màn hình thu ngân tại quầy (nhập mã ID, tự nhảy tiền, áp giảm giá VIP, hiển thị hóa đơn).
     - `UC_SanPham.xaml` & `UC_LichSuGia.xaml`: Màn hình quản lý thông tin trang sức và tra cứu lịch sử giá.
     - `UC_NhapKho.xaml` & `UC_KiemKe.xaml`: Màn hình cho thủ kho lập phiếu nhập và đối soát kiểm kê.
     - `UC_KhachHang.xaml`: Màn hình tra cứu và quản lý khách hàng VIP.
     - `UC_BaoHanh.xaml`: Màn hình tiếp nhận và trả hàng bảo hành.
     - `UC_NhanVien.xaml` & `UC_AuditLog.xaml`: Màn hình phân quyền nhân sự và xem nhật ký thao tác.
     - `UC_BaoCaoDoanhThu.xaml`: Màn hình báo cáo doanh thu và hoa hồng nhân viên.
   - Cửa sổ đăng nhập `LoginWindow.xaml` kiểm tra tài khoản và phân quyền chức năng.
2. **Cổng thông tin Web Khách hàng (`BlinkBlink.Web`):**
   - Trang chủ giới thiệu thương hiệu và các bộ sưu tập trang sức.
   - Trang danh mục xem chi tiết thông số mẫu mã (chất liệu, size, giá bán).
   - Trang giỏ hàng trực tuyến cho khách lưu món đồ muốn mua.
   - Trang tra cứu bảo hành và tra cứu điểm tích lũy theo số điện thoại.

---

#### Giai đoạn 5: Kiểm thử Tự động & Tinh chỉnh (`tests/`)
1. **Kiểm thử đơn vị (`BlinkBlink.UnitTests`):** Kiểm tra tính toán hóa đơn, công thức chiết khấu hạng thẻ, tính tỷ lệ hoa hồng nhân viên.
2. **Kiểm thử tích hợp (`BlinkBlink.IntegrationTests`):** Kiểm tra giao dịch toàn vẹn trên SQL Server (khi lưu đơn hàng thành công thì đồng thời trừ tồn kho, cập nhật trạng thái món đồ và cộng điểm khách hàng; nếu một khâu thất bại thì toàn bộ giao dịch phải tự động Rollback an toàn).
