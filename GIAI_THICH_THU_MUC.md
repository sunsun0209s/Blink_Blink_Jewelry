# GIẢI THÍCH CHI TIẾT CẤU TRÚC THƯ MỤC DỰ ÁN BLINK BLINK JEWELRY

Tài liệu này giải thích cụ thể: **mỗi thư mục hiện tại chứa gì** và **trong tương lai sẽ được viết thêm những gì** theo đúng kiến trúc phân tầng (Clean Architecture / N-Tier) của dự án.

---

## 1. THƯ MỤC NGUỒN CHÍNH: `src/`

Đây là nơi chứa toàn bộ mã nguồn của hệ thống, được chia thành 5 dự án con độc lập nhưng liên kết chặt chẽ với nhau:

---

### 1.1. `src/BlinkBlink.Domain/` (Tầng Lõi Nghiệp Vụ - Không phụ thuộc bất kỳ ai)
Là tầng quan trọng nhất của toàn bộ hệ thống. Tầng này nắm giữ bản chất nghiệp vụ kinh doanh trang sức và không phụ thuộc vào bất kỳ thư viện hay công nghệ giao diện nào.

* **Hiện tại đang có:**
  * `Common/`: Các lớp nền tảng dùng chung (như `BaseEntity` định nghĩa ID, ngày tạo).
  * `Entities/`: Thư mục chứa các đối tượng nghiệp vụ.
  * `Enums/`: Thư mục chứa các kiểu liệt kê trạng thái.
  * `Exceptions/`: Thư mục chứa các lỗi nghiệp vụ riêng.
  * `Interfaces/`: Thư mục chứa các bản thiết kế hợp đồng truy vấn (Repository Interfaces).
* **Tương lai sẽ viết thêm:**
  * `Entities/`: 19 lớp thực thể nghiệp vụ (Rich Domain Model):
    * `SanPhamTrangSuc.cs`: Quản lý mã ID độc bản, trọng lượng vàng, tiền công, tiền đá, số lượng tồn.
    * `BangGiaVang.cs` & `LoaiVang.cs`: Quản lý biểu giá vàng theo ngày (24K, 18K, 14K...) làm căn cứ tính giá bán.
    * `HoaDon.cs` & `ChiTietHoaDon.cs`: Tự quản lý việc thêm bớt món hàng, tự tính tổng tiền, bảo vệ không cho sửa hóa đơn đã thanh toán.
    * `PhieuThuMua.cs` & `ChiTietThuMua.cs`: Quản lý nghiệp vụ thu mua vàng cũ và hao mòn.
    * `KhachHang.cs` & `TheThanhVien.cs`: Quản lý thông tin khách hàng, số điểm tích lũy và nâng hạng VIP (Đồng, Bạc, Vàng, Kim Cương).
    * `PhieuKiemKe.cs`, `PhieuNhapKho.cs`, `PhieuBaoHanh.cs`...
  * `Enums/`: `TrangThaiHoaDon.cs` (Đang tạo, Đã thanh toán, Đã hủy), `HangTheVip.cs` (Dong, Bac, Vang, KimCuong), `LoaiGiaoDich.cs`.
  * `Exceptions/`: `SanPhamDaBanException.cs`, `ChuaCapNhatGiaVangException.cs`, `KhongDuTonKhoException.cs`.
  * `Interfaces/`: `IRepository<T>.cs`, `IUnitOfWork.cs`, `IInvoiceRepository.cs`, `IProductRepository.cs`.

---

### 1.2. `src/BlinkBlink.Application/` (Tầng Ca Sử Dụng & Dịch Vụ Nghiệp Vụ)
Là tầng điều phối luồng làm việc (Use Cases). Tầng này nhận yêu cầu từ màn hình (Admin hoặc Web), gọi tầng `Domain` để xử lý và phối hợp lưu trữ dữ liệu.

* **Hiện tại đang có:**
  * `Common/`: Chứa các lớp định dạng dữ liệu truyền tải (DTO).
  * `Interfaces/`: Chứa các bản thiết kế dịch vụ nghiệp vụ.
  * `Services/`: Chứa các hàm xử lý logic cụ thể.
  * `Strategies/`: Chứa các thuật toán chiết khấu/giảm giá (Strategy Pattern).
  * `Validators/`: Chứa các quy tắc kiểm tra dữ liệu đầu vào.
* **Tương lai sẽ viết thêm:**
  * `Common/DTOs/`:
    * `TaoHoaDonRequestDto.cs`, `HoaDonResponseDto.cs`.
    * `CapNhatGiaVangDto.cs`, `ThongTinKhachHangDto.cs`, `KiemKeKhoDto.cs`.
  * `Services/`:
    * `PricingService.cs`: Chuyên tính giá bán vàng theo thời giá ngày hôm nay kết hợp tiền công và tiền đá.
    * `InvoiceService.cs`: Điều phối toàn bộ quy trình bán hàng, kiểm tra còn hàng không, cấn trừ tiền vàng cũ nếu có đổi mới, lưu hóa đơn.
    * `BuybackService.cs`: Xử lý tính toán giá trị thu mua trang sức cũ sau khi trừ tỷ lệ hao mòn.
    * `InventoryService.cs`: Xử lý nhập lô hàng mới vào kho, phát sinh mã ID, và thực hiện đối soát kiểm kê kho định kỳ.
    * `CustomerService.cs`: Tự động cộng điểm thưởng cho khách và nâng hạng thẻ thành viên khi đủ hạn mức.
  * `Strategies/`:
    * `BronzeDiscountStrategy.cs`, `GoldDiscountStrategy.cs`, `DiamondDiscountStrategy.cs`: Tính tỷ lệ giảm giá tiền công tương ứng với từng hạng thẻ VIP.

---

### 1.3. `src/BlinkBlink.Infrastructure/` (Tầng Hạ Tầng Kỹ Thuật & Cơ Sở Dữ Liệu)
Chịu trách nhiệm kết nối trực tiếp với Microsoft SQL Server thông qua Entity Framework Core. Tầng này hiện thực hóa các bản thiết kế giao tiếp được khai báo ở tầng `Domain`.

* **Hiện tại đang có:**
  * `Data/`: Chứa cấu hình kết nối CSDL và các bản ghi cấu trúc bảng (Migrations).
  * `Repositories/`: Chứa các lớp truy vấn dữ liệu thực tế từ SQL Server.
  * `Reporting/`: Chứa dịch vụ chuẩn bị dữ liệu hiển thị hóa đơn và báo cáo.
  * `Security/`: Chứa các thuật toán mã hóa mật khẩu và bảo mật.
* **Tương lai sẽ viết thêm:**
  * `Data/`:
    * `AppDbContext.cs`: Khai báo 19 bảng CSDL trên SQL Server.
    * `Configurations/`: Các file cấu hình chi tiết cho từng bảng (khóa chính, khóa ngoại, độ dài ký tự, chỉ mục Index tăng tốc tìm kiếm, ràng buộc 3NF).
    * `Migrations/`: Các file mã nguồn tự động sinh bảng và cập nhật cấu trúc database trên SQL Server.
  * `Repositories/`:
    * `GenericRepository<T>.cs`: Các hàm dùng chung (Thêm, Sửa, Xóa, Lấy theo ID, Lấy danh sách).
    * `InvoiceRepository.cs`, `ProductRepository.cs`: Các câu truy vấn đặc thù (tìm trang sức theo ID, tìm hóa đơn theo ngày).
  * `Reporting/`:
    * `InvoicePreviewService.cs`: Chuẩn bị định dạng dữ liệu hóa đơn điện tử để hiển thị trực quan lên màn hình.
  * `Security/`:
    * `PasswordHasher.cs`: Băm mật khẩu người dùng (bằng Argon2 hoặc BCrypt) trước khi lưu vào CSDL, tuyệt đối không lưu mật khẩu thô.

---

### 1.4. `src/BlinkBlink.Admin/` (Giao Diện Quản Trị & Bán Hàng - Ứng Dụng Desktop WPF)
Là chương trình cài đặt trên máy tính Windows tại cửa hàng, dành riêng cho 3 đối tượng nội bộ: **Quản lý**, **Nhân viên bán hàng (Thu ngân)** và **Thủ kho**. Sử dụng công nghệ WPF (.NET 10) với mô hình MVVM (Model-View-ViewModel).

* **Hiện tại đang có:**
  * `App.xaml`, `App.xaml.cs`: Điểm khởi động ứng dụng và cấu hình tiêm phụ thuộc (Dependency Injection).
  * `MainWindow.xaml`: Cửa sổ khung làm việc chính.
  * `Views/`: Thư mục chứa các màn hình giao diện.
  * `ViewModels/`: Thư mục chứa mã điều khiển giao diện.
  * `Services/`: Các dịch vụ hỗ trợ riêng cho giao diện Desktop (như chuyển trang, thông báo popup).
* **Tương lai sẽ viết thêm:**
  * `Views/`:
    * `FrmLogin.xaml`: Màn hình đăng nhập và chuyển hướng theo vai trò (Quản lý / Thu ngân / Thủ kho).
    * `FrmPOS.xaml`: Màn hình bán hàng tại quầy (nhập mã ID sản phẩm, tự động tính tiền theo giá ngày, cấn trừ đổi cũ, hiển thị hóa đơn).
    * `FrmThuMua.xaml`: Màn hình thẩm định và lập phiếu thu mua trang sức cũ từ khách.
    * `FrmBangGiaVang.xaml`: Màn hình cho Quản lý cập nhật đơn giá mua/bán các loại vàng trong ngày.
    * `FrmKhoHang.xaml`: Màn hình cho Thủ kho lập phiếu nhập hàng mới, cấp mã ID cho từng món.
    * `FrmKiemKe.xaml`: Màn hình kiểm kê hàng thực tế và lập biên bản chênh lệch.
    * `FrmBaoCao.xaml`: Màn hình cho Quản lý xem biểu đồ doanh thu, chi phí, lợi nhuận gộp theo ngày/tháng.
  * `ViewModels/`:
    * `LoginViewModel.cs`, `PosViewModel.cs`, `GoldPriceViewModel.cs`...: Lắng nghe sự kiện người dùng bấm nút, truyền dữ liệu 2 chiều (Data Binding) và gọi xuống `Application Layer`.

---

### 1.5. `src/BlinkBlink.Web/` (Cổng Thông Tin Cho Khách Hàng - Web ASP.NET Core MVC)
Là website chạy trên trình duyệt web, dành riêng cho **Khách hàng** bên ngoài truy cập từ xa. Dùng chung tầng `Application` và CSDL với ứng dụng Desktop mà không cần viết lại logic.

* **Hiện tại đang có:**
  * `Program.cs`: Cấu hình máy chủ web, định tuyến URL và nạp CSDL.
  * `Controllers/`: Nhận yêu cầu từ trình duyệt web.
  * `Views/`: Giao diện web hiển thị cho người xem (HTML/Razor).
  * `Models/`: Dữ liệu hiển thị ra trang web.
  * `wwwroot/`: Chứa CSS, JavaScript, thư viện Bootstrap và hình ảnh tĩnh.
* **Tương lai sẽ viết thêm:**
  * `Controllers/`:
    * `HomeController.cs`: Trang chủ giới thiệu cửa hàng Blink Blink Jewelry.
    * `ProductsController.cs`: Xem danh mục mẫu mã trang sức (nhẫn, dây chuyền, bông tai, kiềng...).
    * `GoldPricesController.cs`: Bảng niêm yết biểu giá vàng trong ngày cập nhật thời gian thực cho khách theo dõi.
    * `WarrantyController.cs`: Trang cho khách nhập mã ID món đồ để tra cứu tình trạng bảo hành và thời hạn cam kết tuổi vàng.
    * `MembershipController.cs`: Trang cho khách nhập số điện thoại để tra cứu số điểm tích lũy và thứ hạng thẻ VIP của mình.
  * `Views/`: Các giao diện trang web tương ứng bằng file `.cshtml`.

---

## 2. THƯ MỤC KIỂM THỬ: `tests/`

Dùng để viết các đoạn mã tự động kiểm tra xem chương trình có bị lỗi tính toán hay lỗi dữ liệu hay không trước khi ghép code vào hệ thống.

* **`tests/BlinkBlink.UnitTests/`**:
  * *Tương lai:* Kiểm tra độc lập các thuật toán trong bộ nhớ:
    * Kiểm tra công thức tính giá vàng thời gian thực có đúng từng chữ số thập phân không.
    * Kiểm tra thuật toán trừ tỷ lệ hao mòn khi thu mua vàng cũ.
    * Kiểm tra logic tự động nâng hạng thẻ VIP khi cộng dồn điểm thưởng.
* **`tests/BlinkBlink.IntegrationTests/`**:
  * *Tương lai:* Kiểm tra kết nối với CSDL thật:
    * Kiểm tra giao dịch toàn vẹn (Transaction ACID): khi lập hóa đơn thì CSDL có trừ đúng 1 sản phẩm trong kho và tăng đúng số điểm cho khách không. Nếu một bước lỗi thì toàn bộ giao dịch có tự động hủy (Rollback) an toàn hay không.

---

## 3. CÁC TỆP TIN Ở THƯ MỤC GỐC (ROOT)

* **`.gitignore`**:
  * Chứa danh sách các tệp rác sinh ra trong quá trình chạy thử (như thư mục `bin/`, `obj/`, bộ nhớ tạm của Visual Studio để Git tự động bỏ qua, không bao giờ gửi lên GitHub.
* **`BlinkBlinkJewelry.sln` & `BlinkBlinkJewelry.slnx`**:
  * Là tệp Solution tổng thể của đồ án. Khi mở tệp này bằng Visual Studio, JetBrains Rider hoặc VS Code, toàn bộ 7 dự án thành phần sẽ được nạp đồng thời và liên kết với nhau.
* **`README.md`**:
  * Trang tóm tắt giới thiệu chung về đồ án, danh sách thành viên trong nhóm, hiển thị trực tiếp khi mở kho lưu trữ trên GitHub.
* **`WORKFLOW.md`**:
  * Bảng hướng dẫn các câu lệnh Git ngắn gọn hằng ngày (tạo nhánh, lưu code, đẩy code, yêu cầu gộp code) dành cho 4 thành viên trong nhóm làm việc cùng nhau mà không bị ghi đè hay xung đột mã nguồn.
