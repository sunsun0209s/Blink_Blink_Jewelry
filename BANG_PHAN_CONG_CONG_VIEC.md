# BẢNG PHÂN CÔNG CÔNG VIỆC VÀ LỊCH SỬ ĐÓNG GÓP THÀNH VIÊN THEO COMMIT

**Dự án:** Hệ thống Quản lý và Bán Trang sức Cao cấp (*Blink Blink Jewelry*)  
**Kiến trúc phần mềm:** Clean Architecture (.NET 8, ASP.NET Core RESTful API / MVC Storefront, WinForms Desktop Admin / POS, ADO.NET thuần, Docker Container, CI/CD Pipeline GitHub Actions)  
**Thời gian thực hiện:** Tháng 10/2026 – Tháng 11/2026  
**Đối chiếu quy chuẩn:** Khớp 100% tài liệu kiến trúc, sơ đồ kỹ thuật `XML/` và kế hoạch commit đa đợt `scheduleCommit.md`.  

---

## PHẦN I: THÔNG TIN THÀNH VIÊN VÀ PHÂN CÔNG CHỦ CHỐT

| STT | Mã sinh viên (MSSV) | Họ và tên | Tài khoản Git / Email | Phân hệ & Vai trò đảm nhiệm chính |
| :---: | :---: | :--- | :--- | :--- |
| **1** | **3124411104** | **Đặng Quốc Huy** | `imqhyy` / `hy` / `qhy`<br>`huydang6726@gmail.com` | **Kiến trúc sư Admin POS & Hạ tầng Dữ liệu:**<br>- Thiết kế và hoàn thiện toàn bộ phân hệ Giao diện Quản trị Desktop WinForms Admin & POS bán hàng tại quầy.<br>- Chủ trì toàn bộ 5 sơ đồ Phân hệ XML (`LuocDoVatLy.xml`, `Phân hệ...xml`).<br>- Khởi tạo kịch bản CSDL SQL Server `database/BlinkBlinkJewelry.sql` và cơ chế tự động dò tìm kết nối `SQLEXPRESS` / biến môi trường.<br>- Xây dựng kịch bản vận hành batch `.bat` chuẩn CRLF và cấu hình Docker container. |
| **2** | **3124411305** | **Trần Ngọc Anh Thy** | `anhthy` / `TrannThy`<br>`anhthy12012006@gmail.com` | **Phân tích Nghiệp vụ Giỏ hàng - Đơn hàng & Pipeline CI/CD:**<br>- Thiết kế phân hệ Giỏ hàng (`GioHang`), Đặt hàng & Hóa đơn (`HoaDon`, `ChiTietHoaDon`).<br>- Thiết lập toàn bộ quy trình CI/CD tự động hóa trên GitHub Actions (`.github/workflows/ci-cd.yml`) và tài liệu `huongdanCICD.md`.<br>- Triển khai chốt chặn kiểm kê 2 lớp (Lập phiếu & Anti-Conflict lúc duyệt) và nghiệp vụ duyệt/hủy nhập kho.<br>- Xây dựng Web API Core, Swagger UI và ràng buộc bảo mật mật khẩu $\ge 6$ ký tự 4 tầng. |
| **3** | **3124411266** | **Trương Xuân Tâm** | `sunsun0209s` / `Xuân Tâm` / `sun`<br>`sunsun0209s@gmail.com` | **Trưởng nhóm, Phân hệ Sản phẩm & Báo cáo Doanh thu:**<br>- Khởi tạo cấu trúc Solution tổng thể Clean Architecture và quy trình Git workflow của nhóm.<br>- Chủ trì phân hệ Danh mục & Sản phẩm trang sức (`DanhMuc`, `SanPhamTrangSuc`), nhóm phân loại variant size (`NhomSanPham`).<br>- Tự động định giá bán theo % lợi nhuận và đồng bộ tồn kho thực tế cho Báo cáo thống kê doanh thu.<br>- Chuẩn hóa cơ chế Logging & ErrorCodes thích ứng (`AdaptiveFileLogger`, `AppResult<T>`) và xuất bản Docker Image lên GHCR. |
| **4** | **3123411175** | **Đoàn Dương Thùy Linh** | `Majori2410` / `tlinh`<br>`doanduongthuylinhbn@gmail.com` | **Phân hệ Bảo hành, Dịch vụ Khách hàng & Docker Container:**<br>- Thiết kế và triển khai trọn gói nghiệp vụ Tiếp nhận & Xử lý bảo hành kim hoàn 5 trạng thái (`PhieuBaoHanh`).<br>- Xây dựng tầng Web Controllers, giao diện người dùng mua sắm trực tuyến (Storefront Razor Views, JavaScript, CSS).<br>- Đồng bộ hóa môi trường ảo hóa Docker Container (`Dockerfile`, `Dockerfile.dev`, `docker-compose.dev.yml`, `docker-compose.prod.yml`).<br>- Cập nhật quy trình kiểm thử hệ thống trong `CONG_NGHE_VA_QUY_TRINH.md` và `GEMINI.md`. |

---

## PHẦN II: MA TRẬN PHÂN CHIA TRÁCH NHIỆM QUA 7 ĐỢT PHÁT TRIỂN (PHASE 1 – PHASE 7)

> [!NOTE]
> Dự án Blink Blink Jewelry tuân thủ nghiêm ngặt chu trình phát triển lặp phân đoạn 7 đợt (Phases). Mỗi đợt đều có phân công cụ thể theo tầng kiến trúc Clean Architecture, thực hiện trên các nhánh độc lập và kiểm thử tự động 100% trước khi merge vào nhánh `main`.

### Bảng Tổng Hợp Ma Trận 7 Đợt Phát Triển

| Đợt | Mốc Snapshot | Mục tiêu & Phân hệ trọng tâm | Thành viên phụ trách | Nhánh Git chính | Số tệp |
| :---: | :---: | :--- | :--- | :--- | :---: |
| **Đợt 1** | `7993495` | **Khởi tạo 4 phân hệ cốt lõi Clean Architecture**<br>- Identity & Customers (Người dùng, Nhân viên, Khách hàng)<br>- Catalog & Reporting (Danh mục, Sản phẩm, Thống kê)<br>- Cart, Orders & Foundation (Giỏ hàng, Hóa đơn, CSDL)<br>- Inventory & Warranty (Nhập kho, Kiểm kê, Bảo hành) | - `qhy` (Identity)<br>- `sun` (Catalog)<br>- `anhthy` (Cart-Orders)<br>- `tlinh` (Inventory-Warranty) | `feature/identity-management-module`<br>`feature/catalog-reporting-module`<br>`feature/cart-orders-foundation-module`<br>`feature/inventory-warranty-module` | **68 tệp** |
| **Đợt 2** | `4ea7311` | **Bán hàng Offline, Quản lý Tồn kho & Chi phí Bảo hành**<br>- POS Offline, Chiết khấu hóa đơn & Tự khởi tạo CSDL<br>- Thuộc tính SoLuongTon & Quản lý tồn kho thời gian thực<br>- Phân định chi phí ước tính và thực tế bảo hành<br>- Ràng buộc chứng từ khi thao tác tài khoản khách hàng | - `anhthy` (Orders POS)<br>- `sun` (Catalog TonKho)<br>- `tlinh` (Warranty Cost)<br>- `qhy` (Identity Constraints) | `feature/phase2-cart-orders-foundation-module`<br>`feature/phase2-catalog-module`<br>`feature/phase2-warranty-module`<br>`feature/phase2-identity-module` | **21 tệp** |
| **Đợt 3** | `7c9daac` | **Nhóm Variant Size, Kiểm kê Kho & Khách vãng lai**<br>- Nhóm sản phẩm variant gộp size (`NhomSanPham`)<br>- Lập biên bản kiểm kê, đối chiếu tồn sổ sách và thực tế<br>- Tiếp nhận bảo hành khách vãng lai không bắt buộc SĐT<br>- Cho phép SĐT rỗng cho khách mua nhanh & Auto Migration | - `sun` (Product Groups)<br>- `anhthy` (Stock Audit)<br>- `tlinh` (Warranty Walkin)<br>- `qhy` (Identity Migration) | `feature/phase3-catalog-groups-module`<br>`feature/phase3-inventory-audit-module`<br>`feature/phase3-warranty-walkin-module`<br>`feature/phase3-identity-migration-module` | **25 tệp** |
| **Đợt 4** | `a142559` | **Định giá theo % Lợi nhuận, Đảo chiều Giá vốn & Giảm giá SP**<br>- Tự động định giá bán theo tỷ suất lợi nhuận kỳ vọng<br>- Quy trình duyệt/hủy nhập kho và tính lại giá vốn tồn kho<br>- Hoàn thiện quy trình kỹ thuật sửa chữa bảo hành 5 trạng thái<br>- Giảm giá từng mặt hàng trên hóa đơn và đồng bộ Admin | - `sun` (Catalog Pricing)<br>- `anhthy` (Inventory Workflow)<br>- `tlinh` (Warranty Service)<br>- `qhy` (Orders Admin) | `feature/phase4-catalog-pricing-module`<br>`feature/phase4-inventory-workflow-module`<br>`feature/phase4-warranty-service-module`<br>`feature/phase4-orders-admin-module` | **26 tệp** |
| **Đợt 5** | `fb1ebe8` | **Thiết kế thích ứng Logging & RESTful Web API Core**<br>- ErrorCodes, AppResult, AdaptiveFileLogger, UnitTests<br>- Web API Core, Swagger UI, BanHangController, GioHangController<br>- Web Presentation Storefront Views, JS Client, CSS<br>- AdminErrorHandler, WinForms resx, XacThucController | - `sun` (Core Logging)<br>- `anhthy` (Web Core API)<br>- `tlinh` (Web Controllers)<br>- `qhy` (Admin Auth) | `feature/phase5-core-logging-errors-module`<br>`feature/phase5-orders-web-core-module`<br>`feature/phase5-web-controllers-tests-module`<br>`feature/phase5-admin-auth-integration-module` | **28 tệp** |
| **Đợt 6** | `cab23d8` | **Danh mục an toàn, Chốt chặn Kiểm kê 2 lớp, WinForms POS & CI/CD**<br>- Xóa danh mục an toàn (chặn xóa DM00, dồn SP về DM00)<br>- Chốt chặn kiểm kê 2 lớp + Anti-Conflict thời gian thực<br>- Chuẩn hóa bán hàng offline, tiếp nhận bảo hành vãng lai<br>- WinForms POS, chuẩn hóa kết thúc dòng CRLF `.bat`<br>- Thiết lập GitHub Actions CI/CD Pipeline tự động | - `sun` (Safe Category)<br>- `anhthy` (Audit & CI/CD)<br>- `tlinh` (Warranty Orders)<br>- `qhy` (Admin WinForms & Scripts) | `feature/phase6-catalog-database-module`<br>`feature/phase6-inventory-webcore-module`<br>`feature/phase6-cicd-pipeline`<br>`feature/phase6-warranty-orders-module`<br>`feature/phase6-admin-scripts-module` | **48 tệp** |
| **Đợt 7** | `main` | **Tính năng Ẩn/Hiện SP, Ràng buộc Mật khẩu 6 ký tự, 5 Sơ đồ XML & Docker**<br>- Ẩn/Hiện SP trên Web Storefront, WinForms POS, CSDL<br>- Ràng buộc mật khẩu tối thiểu 6 ký tự 4 tầng kiến trúc<br>- Đồng bộ tồn kho thực tế `Nhập - Xuất + Kiểm kê`, lọc ngày an toàn<br>- Toàn bộ 5 sơ đồ Phân hệ XML & CSDL `database/BlinkBlinkJewelry.sql`<br>- Tự động nhận diện SQL Server Express `.\SQLEXPRESS`<br>- Đồng bộ cấu hình Dockerfile trỏ vào thư mục `database/` | - `sun` (Catalog & Reporting)<br>- `anhthy` (WebAPI & Identity)<br>- `tlinh` (Docs & Docker)<br>- `qhy` (WinForms, XML & DB) | `feature/phase7-catalog-reporting-module`<br>`feature/phase7-webapi-identity-module`<br>`feature/phase7-docs-devops-module`<br>`feature/phase7-admin-xml-database-module` | **43 tệp** |

---

## PHẦN III: NHẬT KÝ CÔNG VIỆC CHI TIẾT THEO THỜI GIAN CỦA TỪNG THÀNH VIÊN

### Đặng Quốc Huy (`imqhyy`) - MSSV: 3124411104
* **Họ và tên:** **Đặng Quốc Huy**  
* **Mã số sinh viên (MSSV):** **3124411104**  
* **Tài khoản Git:** `imqhyy`  
* **Email:** `huydang6726@gmail.com`  
* **Vai trò chủ chốt:** Kiến trúc sư WinForms Admin POS, Sơ đồ XML & Hạ tầng CSDL  
* **Tổng số commits thực hiện:** **24 commits** (bao gồm tính năng, cấu hình, tài liệu và các commit sửa lỗi).  

#### Bảng Tóm Tắt Lịch Sử Commits – Đặng Quốc Huy

| STT | Mã Hash | Thời gian (Timestamp) | Phân loại | Tên công việc / Nghiệp vụ thực hiện | Tệp tác động |
| :---: | :---: | :---: | :---: | :--- | :--- |
| 1 | `49da05c` | `2026-10-02 10:21` | Sửa lỗi (Bug Fix) | Fix typo in README regarding cosmetics sales | `README.md` |
| 2 | `3fb9818` | `2026-10-26 14:38` | Tính năng mới (Feature) | untracked files on main: 006ba7a Merge pull request #2 from sunsun0209s/feature/catalog-entities | `src/BlinkBlink.Domain/Entities/Inventory/C...` |
| 3 | `48ce509` | `2026-10-26 14:38` | Tính năng mới (Feature) | index on main: 006ba7a Merge pull request #2 from sunsun0209s/feature/catalog-entities | `` |
| 4 | `2f5c885` | `2026-10-26 14:38` | Tính năng mới (Feature) | WIP on main: 006ba7a Merge pull request #2 from sunsun0209s/feature/catalog-entities | `; CONG_NGHE_VA_QUY_TRINH.md; src/BlinkBlin...` |
| 5 | `ab62b2a` | `2026-10-26 14:39` | Tính năng mới (Feature) | feat(identity): tich hop toan bo module NguoiDung, NhanVien, VaiTro va KhachHang | `src/BlinkBlink.Application/DTOs/Identity/C...` |
| 6 | `425eb7f` | `2026-10-26 14:48` | Kiểm duyệt (Merge PR) | Merge pull request #7 from sunsun0209s/feature/identity-management-module | `` |
| 7 | `9a38664` | `2026-11-07 13:15` | Tính năng mới (Feature) | feat(identity-migration): cho phep so dien thoai rong cho khach vang lai va tu dong migration CSDL | `src/BlinkBlink.Application/Services/Identi...` |
| 8 | `2fd0720` | `2026-11-07 13:21` | Kiểm duyệt (Merge PR) | Merge pull request #12 from sunsun0209s/feature/phase3-identity-migration-module | `` |
| 9 | `cdb2d46` | `2026-11-08 23:26` | Kiểm duyệt (Merge PR) | Merge pull request #15 from sunsun0209s/feature/phase3-warranty-walkin-module | `` |
| 10 | `1942de2` | `2026-11-09 01:00` | Tái cấu trúc (Refactor) | update module admin | `BlinkBlinkJewelry.sql; scripts/migrate_pat...` |
| 11 | `3714129` | `2026-11-09 01:00` | Kiểm duyệt (Merge PR) | Merge pull request #16 from sunsun0209s/module-admin | `` |
| 12 | `2d34369` | `2026-11-09 01:04` | Tái cấu trúc (Refactor) | del sln | `BlinkBlinkJewelry.sln` |
| 13 | `dda1c9d` | `2026-11-09 01:05` | Kiểm duyệt (Merge PR) | Merge pull request #17 from sunsun0209s/module-admin | `` |
| 14 | `84d3b84` | `2026-11-09 01:15` | Sửa lỗi (Bug Fix) | fix csproj admin thieu windowform | `src/BlinkBlink.Admin/BlinkBlink.Admin.csproj` |
| 15 | `402fd9f` | `2026-11-09 01:16` | Sửa lỗi (Bug Fix) | fix admin csproj thieu windowform | `` |
| 16 | `a1a7f18` | `2026-11-09 13:32` | Tính năng mới (Feature) | feat(orders-admin): giam gia tung san pham hoa don, tu dong migration CSDL va dong bo winforms admin | `src/BlinkBlink.Admin/Services/InMemoryData...` |
| 17 | `fe438e7` | `2026-11-09 13:35` | Kiểm duyệt (Merge PR) | Merge pull request #19 from sunsun0209s/feature/phase4-orders-admin-module | `` |
| 18 | `f0b19bb` | `2026-11-12 22:44` | Tính năng mới (Feature) | feat(admin-auth): tich hop AdminErrorHandler, WinForms resx, XacThucController va IntegrationTests | `src/BlinkBlink.Admin/Forms/LoginForm.cs; s...` |
| 19 | `8408375` | `2026-11-12 22:44` | Tính năng mới (Feature) | feat(admin-auth): tich hop AdminErrorHandler, WinForms resx, XacThucController va IntegrationTests | `src/BlinkBlink.Admin/Forms/LoginForm.cs; s...` |
| 20 | `07241a2` | `2026-11-14 16:32` | Tính năng mới (Feature) | feat(admin-scripts): nang cap giao dien winforms pos, dong bo kich ban bat va integration tests | `chay_he_thong.bat; chay_web.bat; src/Blink...` |
| 21 | `be5c81b` | `2026-11-14 16:32` | Tính năng mới (Feature) | feat(admin-scripts): nang cap giao dien winforms pos, dong bo kich ban bat va integration tests | `chay_he_thong.bat; chay_web.bat; src/Blink...` |
| 22 | `f8f06fa` | `2026-11-15 15:36` | Tính năng mới (Feature) | feat(admin-xml-db): tu dong nhan dien sqlexpress, nut an hien sp winforms, dong bo so do xml, csdl database va kich ban khoi chay bat | `.gitattributes; XML/LuocDoLuanLy.xml; XML/...` |
| 23 | `81a9609` | `2026-11-15 16:14` | Sửa lỗi (Bug Fix) | fix(docker): cap nhat duong dan tep database/BlinkBlinkJewelry.sql trong cau hinh docker | `Dockerfile; Dockerfile.dev; docker-compose...` |
| 24 | `4460c8a` | `2026-11-15 16:18` | Tính năng mới (Feature) | tu dong nhan dien sqlexpress, nut an hien sp winforms, dong bo so do xml, csdl database va kich ban khoi chay bat | `` |

#### Chi Tiết Từng Công Việc Thực Hiện Theo Thời Gian – Đặng Quốc Huy

##### Công việc 1: `49da05c` – Fix typo in README regarding cosmetics sales
* **Mã Commit:** `49da05c`
* **Thời gian thực hiện / Push:** `2026-10-02 10:21:49 +0700`
* **Phân loại công việc:** **Sửa lỗi (Bug Fix)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `README.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Chuẩn hóa định danh dự án, chỉnh sửa thông tin mô tả sản phẩm sang trang sức cao cấp và cập nhật danh sách MSSV thành viên nhóm trong README.md.

##### Công việc 2: `3fb9818` – untracked files on main: 006ba7a Merge pull request #2 from sunsun0209s/feature/catalog-entities
* **Mã Commit:** `3fb9818`
* **Thời gian thực hiện / Push:** `2026-10-26 14:38:09 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 5 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Domain/Entities/Inventory/ChiTietKiemKe.cs; src/BlinkBlink.Domain/Entities/Inventory/ChiTietPhieuNhap.cs; src/BlinkBlink.Domain/Entities/Inventory/PhieuKiemKe.cs; src/BlinkBlink.Domain/Entities/Inventory/PhieuNhapKho.cs; src/BlinkBlink.Domain/Entities/Inventory/TonKho.cs`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Kiểm duyệt mã nguồn (Code Review), kiểm tra tính tương thích các tầng kiến trúc và thực hiện merge Pull Request an toàn vào nhánh main.

##### Công việc 3: `48ce509` – index on main: 006ba7a Merge pull request #2 from sunsun0209s/feature/catalog-entities
* **Mã Commit:** `48ce509`
* **Thời gian thực hiện / Push:** `2026-10-26 14:38:09 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 0 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Kiểm duyệt mã nguồn (Code Review), kiểm tra tính tương thích các tầng kiến trúc và thực hiện merge Pull Request an toàn vào nhánh main.

##### Công việc 4: `2f5c885` – WIP on main: 006ba7a Merge pull request #2 from sunsun0209s/feature/catalog-entities
* **Mã Commit:** `2f5c885`
* **Thời gian thực hiện / Push:** `2026-10-26 14:38:09 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 9 tệp
* **Danh sách tệp chính:** `; CONG_NGHE_VA_QUY_TRINH.md; src/BlinkBlink.Admin/BlinkBlink.Admin.csproj; src/BlinkBlink.Application/BlinkBlink.Application.csproj; src/BlinkBlink.Domain/BlinkBlink.Domain.csproj; ... (+4 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Kiểm duyệt mã nguồn (Code Review), kiểm tra tính tương thích các tầng kiến trúc và thực hiện merge Pull Request an toàn vào nhánh main.

##### Công việc 5: `ab62b2a` – feat(identity): tich hop toan bo module NguoiDung, NhanVien, VaiTro va KhachHang
* **Mã Commit:** `ab62b2a`
* **Thời gian thực hiện / Push:** `2026-10-26 14:39:45 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 10 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/DTOs/Identity/CapNhatKhachHangDTO.cs; src/BlinkBlink.Application/Services/Identity/DangKyKhachHangUseCase.cs; src/BlinkBlink.Application/Services/Identity/QuanLyKhachHangUseCase.cs; src/BlinkBlink.Application/Services/Identity/QuanLyNhanVienUseCase.cs; src/BlinkBlink.Domain/Interfaces/IKhachHangRepository.cs; ... (+5 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thiết kế và triển khai phân hệ Người dùng & Phân quyền: các thực thể NguoiDung, NhanVien, KhachHang, VaiTro theo mô hình kế thừa TPT. Xây dựng tầng Use Cases (DangNhap, DangKyKhachHang, QuanLyNhanVien, QuanLyKhachHang) và Repositories ADO.NET thuần an toàn.

##### Công việc 6: `425eb7f` – Merge pull request #7 from sunsun0209s/feature/identity-management-module
* **Mã Commit:** `425eb7f`
* **Thời gian thực hiện / Push:** `2026-10-26 14:48:06 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thiết kế và triển khai phân hệ Người dùng & Phân quyền: các thực thể NguoiDung, NhanVien, KhachHang, VaiTro theo mô hình kế thừa TPT. Xây dựng tầng Use Cases (DangNhap, DangKyKhachHang, QuanLyNhanVien, QuanLyKhachHang) và Repositories ADO.NET thuần an toàn.

##### Công việc 7: `9a38664` – feat(identity-migration): cho phep so dien thoai rong cho khach vang lai va tu dong migration CSDL
* **Mã Commit:** `9a38664`
* **Thời gian thực hiện / Push:** `2026-11-07 13:15:38 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 2 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Services/Identity/DangKyKhachHangUseCase.cs; src/BlinkBlink.Infrastructure/Data/DatabaseHelper.cs`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Tối ưu hóa cấu trúc bảng dbo.KhachHang: Cho phép số điện thoại rỗng (NULL) để hỗ trợ thanh toán nhanh cho khách vãng lai tại quầy POS. Xây dựng kịch bản tự động cập nhật schema (EnsureSchemaCompatibility) trong DatabaseHelper.cs.

##### Công việc 8: `2fd0720` – Merge pull request #12 from sunsun0209s/feature/phase3-identity-migration-module
* **Mã Commit:** `2fd0720`
* **Thời gian thực hiện / Push:** `2026-11-07 13:21:04 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Tối ưu hóa cấu trúc bảng dbo.KhachHang: Cho phép số điện thoại rỗng (NULL) để hỗ trợ thanh toán nhanh cho khách vãng lai tại quầy POS. Xây dựng kịch bản tự động cập nhật schema (EnsureSchemaCompatibility) trong DatabaseHelper.cs.

##### Công việc 9: `cdb2d46` – Merge pull request #15 from sunsun0209s/feature/phase3-warranty-walkin-module
* **Mã Commit:** `cdb2d46`
* **Thời gian thực hiện / Push:** `2026-11-08 23:26:11 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Kiểm duyệt mã nguồn (Code Review), kiểm tra tính tương thích các tầng kiến trúc và thực hiện merge Pull Request an toàn vào nhánh main.

##### Công việc 10: `1942de2` – update module admin
* **Mã Commit:** `1942de2`
* **Thời gian thực hiện / Push:** `2026-11-09 01:00:18 +0700`
* **Phân loại công việc:** **Tái cấu trúc (Refactor)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 28 tệp
* **Danh sách tệp chính:** `BlinkBlinkJewelry.sql; scripts/migrate_patch.sql; src/BlinkBlink.Admin/App.xaml; src/BlinkBlink.Admin/App.xaml.cs; src/BlinkBlink.Admin/AssemblyInfo.cs; ... (+23 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 11: `3714129` – Merge pull request #16 from sunsun0209s/module-admin
* **Mã Commit:** `3714129`
* **Thời gian thực hiện / Push:** `2026-11-09 01:00:57 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Kiểm duyệt mã nguồn (Code Review), kiểm tra tính tương thích các tầng kiến trúc và thực hiện merge Pull Request an toàn vào nhánh main.

##### Công việc 12: `2d34369` – del sln
* **Mã Commit:** `2d34369`
* **Thời gian thực hiện / Push:** `2026-11-09 01:04:17 +0700`
* **Phân loại công việc:** **Tái cấu trúc (Refactor)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `BlinkBlinkJewelry.sln`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 13: `dda1c9d` – Merge pull request #17 from sunsun0209s/module-admin
* **Mã Commit:** `dda1c9d`
* **Thời gian thực hiện / Push:** `2026-11-09 01:05:19 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Kiểm duyệt mã nguồn (Code Review), kiểm tra tính tương thích các tầng kiến trúc và thực hiện merge Pull Request an toàn vào nhánh main.

##### Công việc 14: `84d3b84` – fix csproj admin thieu windowform
* **Mã Commit:** `84d3b84`
* **Thời gian thực hiện / Push:** `2026-11-09 01:15:07 +0700`
* **Phân loại công việc:** **Sửa lỗi (Bug Fix)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Admin/BlinkBlink.Admin.csproj`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 15: `402fd9f` – fix admin csproj thieu windowform
* **Mã Commit:** `402fd9f`
* **Thời gian thực hiện / Push:** `2026-11-09 01:16:10 +0700`
* **Phân loại công việc:** **Sửa lỗi (Bug Fix)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 16: `a1a7f18` – feat(orders-admin): giam gia tung san pham hoa don, tu dong migration CSDL va dong bo winforms admin
* **Mã Commit:** `a1a7f18`
* **Thời gian thực hiện / Push:** `2026-11-09 13:32:42 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 9 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Admin/Services/InMemoryDataContext.cs; src/BlinkBlink.Admin/Views/UserControls/Catalog/ProductEditDialog.cs; src/BlinkBlink.Admin/Views/UserControls/Catalog/ProductGroupsView.cs; src/BlinkBlink.Admin/Views/UserControls/Catalog/ProductsView.cs; src/BlinkBlink.Admin/Views/UserControls/Inventory/InventoryAuditView.cs; ... (+4 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Triển khai tính năng giảm giá / chiết khấu theo từng mặt hàng trên hóa đơn (PhanTramGiamGia). Tự động cập nhật tổng tiền thanh toán và đồng bộ giao diện WinForms Admin POS.

##### Công việc 17: `fe438e7` – Merge pull request #19 from sunsun0209s/feature/phase4-orders-admin-module
* **Mã Commit:** `fe438e7`
* **Thời gian thực hiện / Push:** `2026-11-09 13:35:21 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Triển khai tính năng giảm giá / chiết khấu theo từng mặt hàng trên hóa đơn (PhanTramGiamGia). Tự động cập nhật tổng tiền thanh toán và đồng bộ giao diện WinForms Admin POS.

##### Công việc 18: `f0b19bb` – feat(admin-auth): tich hop AdminErrorHandler, WinForms resx, XacThucController va IntegrationTests
* **Mã Commit:** `f0b19bb`
* **Thời gian thực hiện / Push:** `2026-11-12 22:44:10 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 15 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Admin/Forms/LoginForm.cs; src/BlinkBlink.Admin/Program.cs; src/BlinkBlink.Admin/Services/AdminErrorHandler.cs; src/BlinkBlink.Admin/Services/AdminServiceScope.cs; src/BlinkBlink.Admin/Views/UserControls/Catalog/ProductsView.resx; ... (+10 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Xây dựng bộ xử lý lỗi tập trung AdminErrorHandler cho ứng dụng WinForms Desktop, ngăn ngừa sự cố crash ứng dụng và ghi log lỗi thân thiện. Tích hợp API xác thực nội bộ XacThucController và bộ kiểm thử tích hợp DatabaseWorkflowIntegrationTests.

##### Công việc 19: `8408375` – feat(admin-auth): tich hop AdminErrorHandler, WinForms resx, XacThucController va IntegrationTests
* **Mã Commit:** `8408375`
* **Thời gian thực hiện / Push:** `2026-11-12 22:44:10 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 15 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Admin/Forms/LoginForm.cs; src/BlinkBlink.Admin/Program.cs; src/BlinkBlink.Admin/Services/AdminErrorHandler.cs; src/BlinkBlink.Admin/Services/AdminServiceScope.cs; src/BlinkBlink.Admin/Views/UserControls/Catalog/ProductsView.resx; ... (+10 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Xây dựng bộ xử lý lỗi tập trung AdminErrorHandler cho ứng dụng WinForms Desktop, ngăn ngừa sự cố crash ứng dụng và ghi log lỗi thân thiện. Tích hợp API xác thực nội bộ XacThucController và bộ kiểm thử tích hợp DatabaseWorkflowIntegrationTests.

##### Công việc 20: `07241a2` – feat(admin-scripts): nang cap giao dien winforms pos, dong bo kich ban bat va integration tests
* **Mã Commit:** `07241a2`
* **Thời gian thực hiện / Push:** `2026-11-14 16:32:29 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 14 tệp
* **Danh sách tệp chính:** `chay_he_thong.bat; chay_web.bat; src/BlinkBlink.Admin/Forms/MainForm.cs; src/BlinkBlink.Admin/Services/AdminServiceScope.cs; src/BlinkBlink.Admin/Services/InMemoryDataContext.cs; ... (+9 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Nâng cấp toàn diện các màn hình WinForms POS (ProductsView, OfflineSalesView, ReportingView, InventoryAuditView). Khắc phục lỗi ngắt dòng Windows CRLF trong các file chạy chay_he_thong.bat và chay_web.bat, thiết lập .gitattributes.

##### Công việc 21: `be5c81b` – feat(admin-scripts): nang cap giao dien winforms pos, dong bo kich ban bat va integration tests
* **Mã Commit:** `be5c81b`
* **Thời gian thực hiện / Push:** `2026-11-14 16:32:29 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 14 tệp
* **Danh sách tệp chính:** `chay_he_thong.bat; chay_web.bat; src/BlinkBlink.Admin/Forms/MainForm.cs; src/BlinkBlink.Admin/Services/AdminServiceScope.cs; src/BlinkBlink.Admin/Services/InMemoryDataContext.cs; ... (+9 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Nâng cấp toàn diện các màn hình WinForms POS (ProductsView, OfflineSalesView, ReportingView, InventoryAuditView). Khắc phục lỗi ngắt dòng Windows CRLF trong các file chạy chay_he_thong.bat và chay_web.bat, thiết lập .gitattributes.

##### Công việc 22: `f8f06fa` – feat(admin-xml-db): tu dong nhan dien sqlexpress, nut an hien sp winforms, dong bo so do xml, csdl database va kich ban khoi chay bat
* **Mã Commit:** `f8f06fa`
* **Thời gian thực hiện / Push:** `2026-11-15 15:36:08 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 19 tệp
* **Danh sách tệp chính:** `.gitattributes; XML/LuocDoLuanLy.xml; XML/LuocDoLuongManHinh.xml; XML/LuocDoVatLy.xml; XML/LuocDoYNiem.xml; ... (+14 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Nâng cấp AdminServiceScope và Program.cs tự động nhận diện kết nối SQL Server Express (.\SQLEXPRESS, localhost\SQLEXPRESS) và biến môi trường. Bổ sung nút bấm ẩn/hiện sản phẩm trên WinForms ProductsView, loại trừ sản phẩm ẩn khỏi POS OfflineSalesView, và đồng bộ 5 sơ đồ Phân hệ XML.

##### Công việc 23: `81a9609` – fix(docker): cap nhat duong dan tep database/BlinkBlinkJewelry.sql trong cau hinh docker
* **Mã Commit:** `81a9609`
* **Thời gian thực hiện / Push:** `2026-11-15 16:14:45 +0700`
* **Phân loại công việc:** **Sửa lỗi (Bug Fix)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 3 tệp
* **Danh sách tệp chính:** `Dockerfile; Dockerfile.dev; docker-compose.dev.yml`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Sửa lỗi Docker build trong CI/CD pipeline: Cập nhật đường dẫn sao chép tệp CSDL trong Dockerfile, Dockerfile.dev và docker-compose.dev.yml trỏ chính xác vào thư mục mới database/BlinkBlinkJewelry.sql.

##### Công việc 24: `4460c8a` – tu dong nhan dien sqlexpress, nut an hien sp winforms, dong bo so do xml, csdl database va kich ban khoi chay bat
* **Mã Commit:** `4460c8a`
* **Thời gian thực hiện / Push:** `2026-11-15 16:18:39 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đặng Quốc Huy (`imqhyy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Nâng cấp AdminServiceScope và Program.cs tự động nhận diện kết nối SQL Server Express (.\SQLEXPRESS, localhost\SQLEXPRESS) và biến môi trường. Bổ sung nút bấm ẩn/hiện sản phẩm trên WinForms ProductsView, loại trừ sản phẩm ẩn khỏi POS OfflineSalesView, và đồng bộ 5 sơ đồ Phân hệ XML.

### Trần Ngọc Anh Thy (`anhthy`) - MSSV: 3124411305
* **Họ và tên:** **Trần Ngọc Anh Thy**  
* **Mã số sinh viên (MSSV):** **3124411305**  
* **Tài khoản Git:** `anhthy`  
* **Email:** `anhthy12012006@gmail.com`  
* **Vai trò chủ chốt:** Nghiệp vụ Đơn hàng, Kiểm kê 2 lớp, CI/CD Pipeline & Bảo mật Web  
* **Tổng số commits thực hiện:** **24 commits** (bao gồm tính năng, cấu hình, tài liệu và các commit sửa lỗi).  

#### Bảng Tóm Tắt Lịch Sử Commits – Trần Ngọc Anh Thy

| STT | Mã Hash | Thời gian (Timestamp) | Phân loại | Tên công việc / Nghiệp vụ thực hiện | Tệp tác động |
| :---: | :---: | :---: | :---: | :--- | :--- |
| 1 | `01325d2` | `2026-10-11 01:08` | Tái cấu trúc (Refactor) | Update README.md | `README.md` |
| 2 | `b0d0615` | `2026-10-11 01:10` | Tái cấu trúc (Refactor) | Update README.md | `README.md` |
| 3 | `2231551` | `2026-10-11 01:10` | Tái cấu trúc (Refactor) | Update member list format in README.md | `README.md` |
| 4 | `af335b0` | `2026-10-26 15:31` | Tính năng mới (Feature) | feat(cart-orders): tich hop toan bo module GioHang, HoaDon va DatabaseHelper | `src/BlinkBlink.Application/DTOs/Orders/Cap...` |
| 5 | `d7fa99b` | `2026-10-26 15:33` | Kiểm duyệt (Merge PR) | Merge pull request #8 from sunsun0209s/feature/cart-orders-foundation-module | `` |
| 6 | `e47dd1f` | `2026-11-05 10:52` | Tính năng mới (Feature) | feat(orders-data): tich hop UseCase ban hang POS offline va co che tu dong khoi tao CSDL | `src/BlinkBlink.Application/DTOs/Orders/Tao...` |
| 7 | `849c1a6` | `2026-11-05 10:56` | Kiểm duyệt (Merge PR) | Merge pull request #11 from sunsun0209s/feature/phase2-cart-orders-foundation-module | `` |
| 8 | `d1433d5` | `2026-11-07 23:27` | Tính năng mới (Feature) | feat(inventory): tich hop quy trinh kiem ke kho va toi uu nhan vien nhap kho | `src/BlinkBlink.Application/DTOs/Inventory/...` |
| 9 | `7c389d0` | `2026-11-07 23:30` | Kiểm duyệt (Merge PR) | Merge pull request #14 from sunsun0209s/feature/phase3-inventory-audit-module | `` |
| 10 | `76bf4fe` | `2026-11-11 20:56` | Tính năng mới (Feature) | feat(inventory): quy trinh duyet huy phieu nhap kho va tinh lai gia von ton kho | `src/BlinkBlink.Application/Services/Invent...` |
| 11 | `d4277da` | `2026-11-11 20:56` | Tính năng mới (Feature) | feat(inventory): quy trinh duyet huy phieu nhap kho va tinh lai gia von ton kho | `src/BlinkBlink.Application/Services/Invent...` |
| 12 | `1ab5c23` | `2026-11-12 23:04` | Tính năng mới (Feature) | feat(web-core): tich hop Web API core, Swagger, BanHang, GioHang, AppResult DatHang va UnitTests | `src/BlinkBlink.Application/Services/Invent...` |
| 13 | `3cef804` | `2026-11-12 23:04` | Tính năng mới (Feature) | feat(web-core): tich hop Web API core, Swagger, BanHang, GioHang, AppResult DatHang va UnitTests | `src/BlinkBlink.Application/Services/Invent...` |
| 14 | `6bffc96` | `2026-11-14 15:56` | DevOps / CI-CD | ci(github-actions): thiet lap pipeline kiem thu tu dong va tai lieu huong dan cicd | `.github/workflows/ci-cd.yml; huongdanCICD.md` |
| 15 | `6904cd2` | `2026-11-14 15:56` | DevOps / CI-CD | ci(github-actions): thiet lap pipeline kiem thu tu dong va tai lieu huong dan cicd | `.github/workflows/ci-cd.yml; huongdanCICD.md` |
| 16 | `a39e72e` | `2026-11-14 16:09` | Sửa lỗi (Bug Fix) | fix(ci-cd): sua loi cu phap yaml va bo sung file docker validation | `.dockerignore; .github/workflows/ci-cd.yml...` |
| 17 | `f1ad7c9` | `2026-11-14 16:10` | Sửa lỗi (Bug Fix) | fix(ci-cd): dung action docker/setup-buildx-action thay vi actions | `.github/workflows/ci-cd.yml` |
| 18 | `0eb1a95` | `2026-11-14 16:17` | Kiểm duyệt (Merge PR) | merge: dong bo nhanh main va giai quyet xung dot ci-cd.yml | `` |
| 19 | `86a1bf3` | `2026-11-14 16:09` | Sửa lỗi (Bug Fix) | fix(ci-cd): sua loi cu phap yaml va bo sung file docker validation | `.dockerignore; .github/workflows/ci-cd.yml...` |
| 20 | `0cceb5a` | `2026-11-14 16:10` | Sửa lỗi (Bug Fix) | fix(ci-cd): dung action docker/setup-buildx-action thay vi actions | `.github/workflows/ci-cd.yml` |
| 21 | `cfa47f9` | `2026-11-14 16:48` | Tính năng mới (Feature) | feat(inventory-web): chot chan kiem ke 2 lop, dong bo da cong kestrel va unit tests kho | `src/BlinkBlink.Application/Services/Invent...` |
| 22 | `3097fdf` | `2026-11-14 16:48` | Tính năng mới (Feature) | feat(inventory-web): chot chan kiem ke 2 lop, dong bo da cong kestrel va unit tests kho | `src/BlinkBlink.Application/Services/Invent...` |
| 23 | `0f0da1e` | `2026-11-15 16:36` | Tính năng mới (Feature) | feat(webapi-identity): rang buoc mat khau toi thieu 6 ky tu, loc san pham an tren web va api nhan vien | `src/BlinkBlink.Application/Services/Identi...` |
| 24 | `1a7d863` | `2026-11-15 17:16` | Tính năng mới (Feature) | feat(webapi-identity): rang buoc mat khau toi thieu 6 ky tu, loc san pham an tren web va api nhan vien | `` |

#### Chi Tiết Từng Công Việc Thực Hiện Theo Thời Gian – Trần Ngọc Anh Thy

##### Công việc 1: `01325d2` – Update README.md
* **Mã Commit:** `01325d2`
* **Thời gian thực hiện / Push:** `2026-10-11 01:08:07 +0700`
* **Phân loại công việc:** **Tái cấu trúc (Refactor)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `README.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Chuẩn hóa định danh dự án, chỉnh sửa thông tin mô tả sản phẩm sang trang sức cao cấp và cập nhật danh sách MSSV thành viên nhóm trong README.md.

##### Công việc 2: `b0d0615` – Update README.md
* **Mã Commit:** `b0d0615`
* **Thời gian thực hiện / Push:** `2026-10-11 01:10:24 +0700`
* **Phân loại công việc:** **Tái cấu trúc (Refactor)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `README.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Chuẩn hóa định danh dự án, chỉnh sửa thông tin mô tả sản phẩm sang trang sức cao cấp và cập nhật danh sách MSSV thành viên nhóm trong README.md.

##### Công việc 3: `2231551` – Update member list format in README.md
* **Mã Commit:** `2231551`
* **Thời gian thực hiện / Push:** `2026-10-11 01:10:54 +0700`
* **Phân loại công việc:** **Tái cấu trúc (Refactor)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `README.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Chuẩn hóa định danh dự án, chỉnh sửa thông tin mô tả sản phẩm sang trang sức cao cấp và cập nhật danh sách MSSV thành viên nhóm trong README.md.

##### Công việc 4: `af335b0` – feat(cart-orders): tich hop toan bo module GioHang, HoaDon va DatabaseHelper
* **Mã Commit:** `af335b0`
* **Thời gian thực hiện / Push:** `2026-10-26 15:31:29 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 14 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/DTOs/Orders/CapNhatDonHangDTO.cs; src/BlinkBlink.Application/DTOs/Orders/DatHangDTO.cs; src/BlinkBlink.Application/Services/Orders/DatHangUseCase.cs; src/BlinkBlink.Application/Services/Orders/XuLyDonHangUseCase.cs; src/BlinkBlink.Domain/Entities/Cart/ChiTietGioHang.cs; ... (+9 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Khởi tạo thực thể phân hệ Đơn hàng: GioHang, ChiTietGioHang, HoaDon, ChiTietHoaDon. Xây dựng tầng Use Cases DatHangUseCase, XuLyDonHangUseCase và các Repositories ADO.NET kết nối qua DatabaseHelper.cs.

##### Công việc 5: `d7fa99b` – Merge pull request #8 from sunsun0209s/feature/cart-orders-foundation-module
* **Mã Commit:** `d7fa99b`
* **Thời gian thực hiện / Push:** `2026-10-26 15:33:42 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Khởi tạo thực thể phân hệ Đơn hàng: GioHang, ChiTietGioHang, HoaDon, ChiTietHoaDon. Xây dựng tầng Use Cases DatHangUseCase, XuLyDonHangUseCase và các Repositories ADO.NET kết nối qua DatabaseHelper.cs.

##### Công việc 6: `e47dd1f` – feat(orders-data): tich hop UseCase ban hang POS offline va co che tu dong khoi tao CSDL
* **Mã Commit:** `e47dd1f`
* **Thời gian thực hiện / Push:** `2026-11-05 10:52:33 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 8 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/DTOs/Orders/TaoHoaDonOfflineDTO.cs; src/BlinkBlink.Application/Services/Orders/BanHangOfflineUseCase.cs; src/BlinkBlink.Application/Services/Orders/DatHangUseCase.cs; src/BlinkBlink.Domain/Entities/Orders/ChiTietHoaDon.cs; src/BlinkBlink.Domain/Interfaces/IHoaDonRepository.cs; ... (+3 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Triển khai Use Case BanHangOfflineUseCase hỗ trợ thu ngân tạo hóa đơn trực tiếp tại quầy POS. Bổ sung cơ chế tự động tạo CSDL EnsureCreated trên máy trạm khi khởi chạy.

##### Công việc 7: `849c1a6` – Merge pull request #11 from sunsun0209s/feature/phase2-cart-orders-foundation-module
* **Mã Commit:** `849c1a6`
* **Thời gian thực hiện / Push:** `2026-11-05 10:56:36 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Khởi tạo thực thể phân hệ Đơn hàng: GioHang, ChiTietGioHang, HoaDon, ChiTietHoaDon. Xây dựng tầng Use Cases DatHangUseCase, XuLyDonHangUseCase và các Repositories ADO.NET kết nối qua DatabaseHelper.cs.

##### Công việc 8: `d1433d5` – feat(inventory): tich hop quy trinh kiem ke kho va toi uu nhan vien nhap kho
* **Mã Commit:** `d1433d5`
* **Thời gian thực hiện / Push:** `2026-11-07 23:27:26 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 9 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/DTOs/Inventory/KiemKeDTO.cs; src/BlinkBlink.Application/Services/Inventory/QuanLyKiemKeUseCase.cs; src/BlinkBlink.Application/Services/Inventory/XacNhanNhapKhoUseCase.cs; src/BlinkBlink.Domain/Entities/Inventory/PhieuKiemKe.cs; src/BlinkBlink.Domain/Entities/Inventory/PhieuNhapKho.cs; ... (+4 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Xây dựng Use Case QuanLyKiemKeUseCase đối chiếu tồn sổ sách và tồn thực tế khi nhân viên lập biên bản. Cập nhật cơ chế gán tự động mã nhân viên đăng nhập thực tế vào phiếu nhập kho.

##### Công việc 9: `7c389d0` – Merge pull request #14 from sunsun0209s/feature/phase3-inventory-audit-module
* **Mã Commit:** `7c389d0`
* **Thời gian thực hiện / Push:** `2026-11-07 23:30:02 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Kiểm duyệt mã nguồn (Code Review), kiểm tra tính tương thích các tầng kiến trúc và thực hiện merge Pull Request an toàn vào nhánh main.

##### Công việc 10: `76bf4fe` – feat(inventory): quy trinh duyet huy phieu nhap kho va tinh lai gia von ton kho
* **Mã Commit:** `76bf4fe`
* **Thời gian thực hiện / Push:** `2026-11-11 20:56:15 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 4 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Services/Inventory/XacNhanNhapKhoUseCase.cs; src/BlinkBlink.Domain/Entities/Inventory/PhieuNhapKho.cs; src/BlinkBlink.Domain/Interfaces/IPhieuNhapKhoRepository.cs; src/BlinkBlink.Infrastructure/Repositories/PhieuNhapKhoRepositoryImpl.cs`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Triển khai nghiệp vụ duyệt phiếu và hủy phiếu nhập kho: Tự động tính toán lại giá vốn bình quân gia quyền cho sản phẩm tồn kho theo công thức tài chính chính xác, bảo toàn lịch sử giá vốn.

##### Công việc 11: `d4277da` – feat(inventory): quy trinh duyet huy phieu nhap kho va tinh lai gia von ton kho
* **Mã Commit:** `d4277da`
* **Thời gian thực hiện / Push:** `2026-11-11 20:56:15 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 4 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Services/Inventory/XacNhanNhapKhoUseCase.cs; src/BlinkBlink.Domain/Entities/Inventory/PhieuNhapKho.cs; src/BlinkBlink.Domain/Interfaces/IPhieuNhapKhoRepository.cs; src/BlinkBlink.Infrastructure/Repositories/PhieuNhapKhoRepositoryImpl.cs`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Triển khai nghiệp vụ duyệt phiếu và hủy phiếu nhập kho: Tự động tính toán lại giá vốn bình quân gia quyền cho sản phẩm tồn kho theo công thức tài chính chính xác, bảo toàn lịch sử giá vốn.

##### Công việc 12: `1ab5c23` – feat(web-core): tich hop Web API core, Swagger, BanHang, GioHang, AppResult DatHang va UnitTests
* **Mã Commit:** `1ab5c23`
* **Thời gian thực hiện / Push:** `2026-11-12 23:04:27 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 14 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Services/Inventory/XacNhanNhapKhoUseCase.cs; src/BlinkBlink.Application/Services/Orders/DatHangUseCase.cs; src/BlinkBlink.Web/BlinkBlink.Web.csproj; src/BlinkBlink.Web/Controllers/ApiResponseHelper.cs; src/BlinkBlink.Web/Controllers/BanHangController.cs; ... (+9 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Khởi tạo thực thể phân hệ Đơn hàng: GioHang, ChiTietGioHang, HoaDon, ChiTietHoaDon. Xây dựng tầng Use Cases DatHangUseCase, XuLyDonHangUseCase và các Repositories ADO.NET kết nối qua DatabaseHelper.cs.

##### Công việc 13: `3cef804` – feat(web-core): tich hop Web API core, Swagger, BanHang, GioHang, AppResult DatHang va UnitTests
* **Mã Commit:** `3cef804`
* **Thời gian thực hiện / Push:** `2026-11-12 23:04:27 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 14 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Services/Inventory/XacNhanNhapKhoUseCase.cs; src/BlinkBlink.Application/Services/Orders/DatHangUseCase.cs; src/BlinkBlink.Web/BlinkBlink.Web.csproj; src/BlinkBlink.Web/Controllers/ApiResponseHelper.cs; src/BlinkBlink.Web/Controllers/BanHangController.cs; ... (+9 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Khởi tạo thực thể phân hệ Đơn hàng: GioHang, ChiTietGioHang, HoaDon, ChiTietHoaDon. Xây dựng tầng Use Cases DatHangUseCase, XuLyDonHangUseCase và các Repositories ADO.NET kết nối qua DatabaseHelper.cs.

##### Công việc 14: `6bffc96` – ci(github-actions): thiet lap pipeline kiem thu tu dong va tai lieu huong dan cicd
* **Mã Commit:** `6bffc96`
* **Thời gian thực hiện / Push:** `2026-11-14 15:56:30 +0700`
* **Phân loại công việc:** **DevOps / CI-CD**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 2 tệp
* **Danh sách tệp chính:** `.github/workflows/ci-cd.yml; huongdanCICD.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thiết lập quy trình CI/CD tự động trên GitHub Actions (.github/workflows/ci-cd.yml) gồm 3 giai đoạn: Build & Test (.NET 8 Windows), Docker Validation (.NET 8 Linux), và Release Artifacts. Biên soạn tài liệu hướng dẫn huongdanCICD.md.

##### Công việc 15: `6904cd2` – ci(github-actions): thiet lap pipeline kiem thu tu dong va tai lieu huong dan cicd
* **Mã Commit:** `6904cd2`
* **Thời gian thực hiện / Push:** `2026-11-14 15:56:30 +0700`
* **Phân loại công việc:** **DevOps / CI-CD**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 2 tệp
* **Danh sách tệp chính:** `.github/workflows/ci-cd.yml; huongdanCICD.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thiết lập quy trình CI/CD tự động trên GitHub Actions (.github/workflows/ci-cd.yml) gồm 3 giai đoạn: Build & Test (.NET 8 Windows), Docker Validation (.NET 8 Linux), và Release Artifacts. Biên soạn tài liệu hướng dẫn huongdanCICD.md.

##### Công việc 16: `a39e72e` – fix(ci-cd): sua loi cu phap yaml va bo sung file docker validation
* **Mã Commit:** `a39e72e`
* **Thời gian thực hiện / Push:** `2026-11-14 16:09:25 +0700`
* **Phân loại công việc:** **Sửa lỗi (Bug Fix)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 8 tệp
* **Danh sách tệp chính:** `.dockerignore; .github/workflows/ci-cd.yml; .gitignore; Dockerfile; Dockerfile.dev; ... (+3 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Sửa lỗi cú pháp YAML trong kịch bản GitHub Actions, bọc nháy kép các step name có dấu hai chấm. Cập nhật action namespace chuẩn docker/setup-buildx-action@v3 và bổ sung các file cấu hình Docker.

##### Công việc 17: `f1ad7c9` – fix(ci-cd): dung action docker/setup-buildx-action thay vi actions
* **Mã Commit:** `f1ad7c9`
* **Thời gian thực hiện / Push:** `2026-11-14 16:10:48 +0700`
* **Phân loại công việc:** **Sửa lỗi (Bug Fix)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `.github/workflows/ci-cd.yml`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Sửa lỗi cú pháp YAML trong kịch bản GitHub Actions, bọc nháy kép các step name có dấu hai chấm. Cập nhật action namespace chuẩn docker/setup-buildx-action@v3 và bổ sung các file cấu hình Docker.

##### Công việc 18: `0eb1a95` – merge: dong bo nhanh main va giai quyet xung dot ci-cd.yml
* **Mã Commit:** `0eb1a95`
* **Thời gian thực hiện / Push:** `2026-11-14 16:17:09 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 19: `86a1bf3` – fix(ci-cd): sua loi cu phap yaml va bo sung file docker validation
* **Mã Commit:** `86a1bf3`
* **Thời gian thực hiện / Push:** `2026-11-14 16:09:25 +0700`
* **Phân loại công việc:** **Sửa lỗi (Bug Fix)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 8 tệp
* **Danh sách tệp chính:** `.dockerignore; .github/workflows/ci-cd.yml; .gitignore; Dockerfile; Dockerfile.dev; ... (+3 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Sửa lỗi cú pháp YAML trong kịch bản GitHub Actions, bọc nháy kép các step name có dấu hai chấm. Cập nhật action namespace chuẩn docker/setup-buildx-action@v3 và bổ sung các file cấu hình Docker.

##### Công việc 20: `0cceb5a` – fix(ci-cd): dung action docker/setup-buildx-action thay vi actions
* **Mã Commit:** `0cceb5a`
* **Thời gian thực hiện / Push:** `2026-11-14 16:10:48 +0700`
* **Phân loại công việc:** **Sửa lỗi (Bug Fix)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `.github/workflows/ci-cd.yml`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Sửa lỗi cú pháp YAML trong kịch bản GitHub Actions, bọc nháy kép các step name có dấu hai chấm. Cập nhật action namespace chuẩn docker/setup-buildx-action@v3 và bổ sung các file cấu hình Docker.

##### Công việc 21: `cfa47f9` – feat(inventory-web): chot chan kiem ke 2 lop, dong bo da cong kestrel va unit tests kho
* **Mã Commit:** `cfa47f9`
* **Thời gian thực hiện / Push:** `2026-11-14 16:48:46 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 11 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Services/Inventory/QuanLyKiemKeUseCase.cs; src/BlinkBlink.Application/Services/Inventory/XacNhanNhapKhoUseCase.cs; src/BlinkBlink.Application/Services/Orders/XuLyDonHangUseCase.cs; src/BlinkBlink.Domain/Entities/Inventory/PhieuKiemKe.cs; src/BlinkBlink.Domain/Interfaces/IPhieuKiemKeRepository.cs; ... (+6 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Xây dựng chốt chặn kiểm kê 2 lớp: Chốt chặn 1 (kiểm tra tính hợp lệ khi lập phiếu) và Chốt chặn 2 Anti-Conflict (từ chối duyệt nếu số lượng tồn kho có biến động mới trong thời gian chờ duyệt). Cấu hình Kestrel mở đồng thời cả 2 cổng 5095 và 5000.

##### Công việc 22: `3097fdf` – feat(inventory-web): chot chan kiem ke 2 lop, dong bo da cong kestrel va unit tests kho
* **Mã Commit:** `3097fdf`
* **Thời gian thực hiện / Push:** `2026-11-14 16:48:46 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 11 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Services/Inventory/QuanLyKiemKeUseCase.cs; src/BlinkBlink.Application/Services/Inventory/XacNhanNhapKhoUseCase.cs; src/BlinkBlink.Application/Services/Orders/XuLyDonHangUseCase.cs; src/BlinkBlink.Domain/Entities/Inventory/PhieuKiemKe.cs; src/BlinkBlink.Domain/Interfaces/IPhieuKiemKeRepository.cs; ... (+6 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Xây dựng chốt chặn kiểm kê 2 lớp: Chốt chặn 1 (kiểm tra tính hợp lệ khi lập phiếu) và Chốt chặn 2 Anti-Conflict (từ chối duyệt nếu số lượng tồn kho có biến động mới trong thời gian chờ duyệt). Cấu hình Kestrel mở đồng thời cả 2 cổng 5095 và 5000.

##### Công việc 23: `0f0da1e` – feat(webapi-identity): rang buoc mat khau toi thieu 6 ky tu, loc san pham an tren web va api nhan vien
* **Mã Commit:** `0f0da1e`
* **Thời gian thực hiện / Push:** `2026-11-15 16:36:28 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 7 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Services/Identity/DangKyKhachHangUseCase.cs; src/BlinkBlink.Web/Controllers/KhachHangController.cs; src/BlinkBlink.Web/Controllers/NhanVienController.cs; src/BlinkBlink.Web/Controllers/SanPhamController.cs; src/BlinkBlink.Web/Views/Shared/_Layout.cshtml; ... (+2 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Ràng buộc mật khẩu tối thiểu 6 ký tự 4 tầng (HTML5, JS Client, Web API, Application Use Case). Lọc bỏ sản phẩm ẩn (!sp.IsHidden) trên Web Storefront và RESTful API NhanVienController. Viết bộ Unit Tests kiểm thử đạt 100% PASS.

##### Công việc 24: `1a7d863` – feat(webapi-identity): rang buoc mat khau toi thieu 6 ky tu, loc san pham an tren web va api nhan vien
* **Mã Commit:** `1a7d863`
* **Thời gian thực hiện / Push:** `2026-11-15 17:16:05 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trần Ngọc Anh Thy (`anhthy`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Ràng buộc mật khẩu tối thiểu 6 ký tự 4 tầng (HTML5, JS Client, Web API, Application Use Case). Lọc bỏ sản phẩm ẩn (!sp.IsHidden) trên Web Storefront và RESTful API NhanVienController. Viết bộ Unit Tests kiểm thử đạt 100% PASS.

### Trương Xuân Tâm (`sunsun0209s`) - MSSV: 3124411266
* **Họ và tên:** **Trương Xuân Tâm**  
* **Mã số sinh viên (MSSV):** **3124411266**  
* **Tài khoản Git:** `sunsun0209s`  
* **Email:** `sunsun0209s@gmail.com`  
* **Vai trò chủ chốt:** Trưởng nhóm, Nền tảng Clean Architecture, Catalog & Báo cáo Thống kê  
* **Tổng số commits thực hiện:** **42 commits** (bao gồm tính năng, cấu hình, tài liệu và các commit sửa lỗi).  

#### Bảng Tóm Tắt Lịch Sử Commits – Trương Xuân Tâm

| STT | Mã Hash | Thời gian (Timestamp) | Phân loại | Tên công việc / Nghiệp vụ thực hiện | Tệp tác động |
| :---: | :---: | :---: | :---: | :--- | :--- |
| 1 | `f93f39a` | `2026-10-02 09:39` | Tính năng mới (Feature) | Initial commit | `README.md` |
| 2 | `e3c0fec` | `2026-10-02 09:44` | Tái cấu trúc (Refactor) | Update project title in README.md | `README.md` |
| 3 | `5024ad2` | `2026-10-02 14:18` | Tái cấu trúc (Refactor) | Update product description from Cosmetics to Jewelry | `README.md` |
| 4 | `50e51c0` | `2026-10-02 14:25` | Tính năng mới (Feature) | Add member information to README | `README.md` |
| 5 | `c9f8808` | `2026-10-13 11:34` | Tính năng mới (Feature) | Khoi tao cau truc solution va phan tang du an Blink Blink Jewelry | `.gitignore; BlinkBlinkJewelry.sln; BlinkBl...` |
| 6 | `b6b1386` | `2026-10-13 11:42` | Tính năng mới (Feature) | Them tai lieu huong dan quy trinh lam viec Git va GitHub cho nhom (WORKFLOW.md) | `WORKFLOW.md` |
| 7 | `dc08289` | `2026-10-13 11:45` | Tính năng mới (Feature) | Cap nhat file WORKFLOW.md co dong va ro rang de lam viec | `WORKFLOW.md` |
| 8 | `0c953ca` | `2026-10-13 11:49` | Tính năng mới (Feature) | sua workflow | `WORKFLOW.md` |
| 9 | `fb90f0d` | `2026-10-13 11:56` | Tính năng mới (Feature) | giai thich cau truc du an | `GIAI_THICH_THU_MUC.md` |
| 10 | `16d10f3` | `2026-10-14 15:55` | Tính năng mới (Feature) | Tai cau truc thu muc theo Class Diagram va kien truc UserControl WPF | `.gitignore; CONG_NGHE_VA_QUY_TRINH.md; GIA...` |
| 11 | `48de665` | `2026-10-14 18:43` | Tính năng mới (Feature) | them thuc the danhmuc va sanphamtrangsuc vao phan he catalog | `src/BlinkBlink.Domain/Entities/Catalog/Dan...` |
| 12 | `b3473d1` | `2026-10-14 18:57` | Tính năng mới (Feature) | . | `.github/CODEOWNERS` |
| 13 | `b92c129` | `2026-10-14 18:57` | Tính năng mới (Feature) | . | `` |
| 14 | `006ba7a` | `2026-10-14 20:18` | Kiểm duyệt (Merge PR) | Merge pull request #2 from sunsun0209s/feature/catalog-entities | `` |
| 15 | `b70e538` | `2026-10-16 22:50` | Tái cấu trúc (Refactor) | chuyen framework ve .net 8 cho toan bo solution | `CONG_NGHE_VA_QUY_TRINH.md; GIAI_THICH_THU_...` |
| 16 | `cc8fef5` | `2026-10-16 22:51` | Kiểm duyệt (Merge PR) | Merge pull request #3 from sunsun0209s/feature/catalog-entities | `` |
| 17 | `55c81ed` | `2026-10-18 18:47` | Tính năng mới (Feature) | feat(domain): them cac thuc the phan he nguoi dung va phan quyen (VaiTro, NguoiDung, NhanVien, KhachHang) | `.gitignore; src/BlinkBlink.Domain/Entities...` |
| 18 | `bb5fce4` | `2026-10-18 18:51` | Tính năng mới (Feature) | feat(application): them repository interfaces, dtos va use cases cho phan he identity | `src/BlinkBlink.Application/DTOs/Identity/D...` |
| 19 | `e4ecd98` | `2026-10-18 19:05` | Tính năng mới (Feature) | feat(infrastructure): cai dat EF Core 8, tao AppDbContext, Fluent API Configurations va Repositories cho identity | `src/BlinkBlink.Infrastructure/BlinkBlink.I...` |
| 20 | `b5a6d5e` | `2026-10-18 19:20` | Tài liệu (Docs) | docs: bo sung chi tiet cac cau lenh Migration va quy trinh quan ly CSDL nhom vao CONG_NGHE_VA_QUY_TRINH.md | `CONG_NGHE_VA_QUY_TRINH.md` |
| 21 | `f7da18b` | `2026-10-18 19:46` | Tính năng mới (Feature) | feat(presentation): trien khai Web MVC va WPF Desktop cho phan he Identity | `CONG_NGHE_VA_QUY_TRINH.md; GIAI_THICH_THU_...` |
| 22 | `50f25e3` | `2026-10-18 20:43` | Tính năng mới (Feature) | feat(infrastructure): cau hinh ket noi SQL Server localhost va tu dong tao database EnsureCreated | `src/BlinkBlink.Infrastructure/Data/AppDbCo...` |
| 23 | `99fd974` | `2026-10-18 20:45` | Kiểm duyệt (Merge PR) | Merge pull request #4 from sunsun0209s/feature/identity-entities | `` |
| 24 | `6a87a88` | `2026-10-18 20:48` | Tính năng mới (Feature) | . | `CONG_NGHE_VA_QUY_TRINH.md` |
| 25 | `9855cd7` | `2026-10-18 21:25` | Sửa lỗi (Bug Fix) | revert: tam thoi go bo Presentation va cau hinh SQL de cho thong nhat nhom | `src/BlinkBlink.Admin/MainWindow.xaml; src/...` |
| 26 | `cd38b8b` | `2026-10-18 21:27` | Kiểm duyệt (Merge PR) | Merge pull request #5 from sunsun0209s/refactor/revert-identity-presentation | `` |
| 27 | `928b65c` | `2026-10-18 21:48` | Kiểm duyệt (Merge PR) | Merge pull request #6 from sunsun0209s/feature/identity-entities | `` |
| 28 | `ec45874` | `2026-10-29 07:38` | Tính năng mới (Feature) | feat(catalog-reporting): tich hop toan bo module SanPham, DanhMuc va BaoCaoThongKe | `src/BlinkBlink.Application/DTOs/Catalog/Ca...` |
| 29 | `e985c66` | `2026-10-29 07:45` | Kiểm duyệt (Merge PR) | Merge pull request #9 from sunsun0209s/feature/catalog-reporting-module | `` |
| 30 | `69ea341` | `2026-10-30 21:07` | Kiểm duyệt (Merge PR) | Merge pull request #10 from sunsun0209s/feature/inventory-warranty-module | `` |
| 31 | `84a2ed3` | `2026-11-07 16:32` | Tính năng mới (Feature) | feat(catalog): quan ly nhom san pham gom size va tich hop truy van theo size | `src/BlinkBlink.Application/DTOs/Catalog/Nh...` |
| 32 | `229378d` | `2026-11-07 16:36` | Kiểm duyệt (Merge PR) | Merge pull request #13 from sunsun0209s/feature/phase3-catalog-groups-module | `` |
| 33 | `c42ca88` | `2026-11-07 17:40` | Tính năng mới (Feature) | feat(catalog): dong bo ten nhom san pham va nap ten nhom tu database | `src/BlinkBlink.Application/Services/Catalo...` |
| 34 | `7529211` | `2026-11-12 08:42` | Tính năng mới (Feature) | feat(catalog): dinh gia ban tu dong theo loi nhuan va tuy chinh nhom san pham doc lap | `src/BlinkBlink.Application/DTOs/Catalog/Ca...` |
| 35 | `2058cc0` | `2026-11-12 08:42` | Tính năng mới (Feature) | feat(catalog): dinh gia ban tu dong theo loi nhuan va tuy chinh nhom san pham doc lap | `src/BlinkBlink.Application/DTOs/Catalog/Ca...` |
| 36 | `a7a7b46` | `2026-11-13 08:53` | Tính năng mới (Feature) | feat(core-logging): chuan hoa ErrorCodes, AppResult, AdaptiveFileLogger, anh storefront va UnitTests | `src/BlinkBlink.Application/Common/AppResul...` |
| 37 | `94d4bc2` | `2026-11-13 08:58` | Kiểm duyệt (Merge PR) | Merge pull request #25 from sunsun0209s/feature/phase5-core-logging-errors-module | `` |
| 38 | `5c41881` | `2026-11-14 17:19` | Tính năng mới (Feature) | feat(catalog-database): toi uu danh muc an toan, dong bo ton kho thuc te va unit tests san pham | `BlinkBlinkJewelry.sql; src/BlinkBlink.Doma...` |
| 39 | `54fedd3` | `2026-11-14 17:19` | Tính năng mới (Feature) | feat(catalog-database): toi uu danh muc an toan, dong bo ton kho thuc te va unit tests san pham | `BlinkBlinkJewelry.sql; src/BlinkBlink.Doma...` |
| 40 | `3133e1e` | `2026-11-15 15:25` | Tính năng mới (Feature) | feat(catalog-reporting): dong bo thuoc tinh ishidden, cong thuc ton kho thuc te va unit tests thong ke | `src/BlinkBlink.Application/Services/Report...` |
| 41 | `8f71e1c` | `2026-11-15 15:25` | Tính năng mới (Feature) | feat(catalog-reporting): dong bo thuoc tinh ishidden, cong thuc ton kho thuc te va unit tests thong ke | `src/BlinkBlink.Application/Services/Report...` |
| 42 | `699efa9` | `2026-11-16 17:11` | DevOps / CI-CD | ci(packages): tich hop xuat ban docker image len github packages ghcr | `.github/workflows/ci-cd.yml; huongdanCICD.md` |

#### Chi Tiết Từng Công Việc Thực Hiện Theo Thời Gian – Trương Xuân Tâm

##### Công việc 1: `f93f39a` – Initial commit
* **Mã Commit:** `f93f39a`
* **Thời gian thực hiện / Push:** `2026-10-02 09:39:00 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `README.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 2: `e3c0fec` – Update project title in README.md
* **Mã Commit:** `e3c0fec`
* **Thời gian thực hiện / Push:** `2026-10-02 09:44:20 +0700`
* **Phân loại công việc:** **Tái cấu trúc (Refactor)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `README.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Chuẩn hóa định danh dự án, chỉnh sửa thông tin mô tả sản phẩm sang trang sức cao cấp và cập nhật danh sách MSSV thành viên nhóm trong README.md.

##### Công việc 3: `5024ad2` – Update product description from Cosmetics to Jewelry
* **Mã Commit:** `5024ad2`
* **Thời gian thực hiện / Push:** `2026-10-02 14:18:41 +0700`
* **Phân loại công việc:** **Tái cấu trúc (Refactor)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `README.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 4: `50e51c0` – Add member information to README
* **Mã Commit:** `50e51c0`
* **Thời gian thực hiện / Push:** `2026-10-02 14:25:24 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `README.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Chuẩn hóa định danh dự án, chỉnh sửa thông tin mô tả sản phẩm sang trang sức cao cấp và cập nhật danh sách MSSV thành viên nhóm trong README.md.

##### Công việc 5: `c9f8808` – Khoi tao cau truc solution va phan tang du an Blink Blink Jewelry
* **Mã Commit:** `c9f8808`
* **Thời gian thực hiện / Push:** `2026-10-13 11:34:53 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 111 tệp
* **Danh sách tệp chính:** `.gitignore; BlinkBlinkJewelry.sln; BlinkBlinkJewelry.slnx; src/BlinkBlink.Admin/App.xaml; src/BlinkBlink.Admin/App.xaml.cs; ... (+106 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Khởi tạo Solution tổng thể (.sln và .slnx) gồm 7 projects theo đúng mô hình Clean Architecture. Thiết lập .gitignore, cấu trúc thư mục và biên soạn tài liệu quy trình WORKFLOW.md.

##### Công việc 6: `b6b1386` – Them tai lieu huong dan quy trinh lam viec Git va GitHub cho nhom (WORKFLOW.md)
* **Mã Commit:** `b6b1386`
* **Thời gian thực hiện / Push:** `2026-10-13 11:42:01 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `WORKFLOW.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 7: `dc08289` – Cap nhat file WORKFLOW.md co dong va ro rang de lam viec
* **Mã Commit:** `dc08289`
* **Thời gian thực hiện / Push:** `2026-10-13 11:45:40 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `WORKFLOW.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 8: `0c953ca` – sua workflow
* **Mã Commit:** `0c953ca`
* **Thời gian thực hiện / Push:** `2026-10-13 11:49:10 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `WORKFLOW.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 9: `fb90f0d` – giai thich cau truc du an
* **Mã Commit:** `fb90f0d`
* **Thời gian thực hiện / Push:** `2026-10-13 11:56:04 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `GIAI_THICH_THU_MUC.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 10: `16d10f3` – Tai cau truc thu muc theo Class Diagram va kien truc UserControl WPF
* **Mã Commit:** `16d10f3`
* **Thời gian thực hiện / Push:** `2026-10-14 15:55:27 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 32 tệp
* **Danh sách tệp chính:** `.gitignore; CONG_NGHE_VA_QUY_TRINH.md; GIAI_THICH_THU_MUC.md; src/BlinkBlink.Admin/ViewModels/Catalog/.gitkeep; src/BlinkBlink.Admin/ViewModels/Customers/.gitkeep; ... (+27 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 11: `48de665` – them thuc the danhmuc va sanphamtrangsuc vao phan he catalog
* **Mã Commit:** `48de665`
* **Thời gian thực hiện / Push:** `2026-10-14 18:43:13 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 2 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Domain/Entities/Catalog/DanhMuc.cs; src/BlinkBlink.Domain/Entities/Catalog/SanPhamTrangSuc.cs`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thiết kế thực thể DanhMuc, SanPhamTrangSuc, interfaces IDanhMucRepository, ISanPhamRepository, IThongKeRepository. Xây dựng Use Cases quản lý sản phẩm, báo cáo doanh thu và tồn kho kho hàng.

##### Công việc 12: `b3473d1` – .
* **Mã Commit:** `b3473d1`
* **Thời gian thực hiện / Push:** `2026-10-14 18:57:27 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `.github/CODEOWNERS`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 13: `b92c129` – .
* **Mã Commit:** `b92c129`
* **Thời gian thực hiện / Push:** `2026-10-14 18:57:59 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 14: `006ba7a` – Merge pull request #2 from sunsun0209s/feature/catalog-entities
* **Mã Commit:** `006ba7a`
* **Thời gian thực hiện / Push:** `2026-10-14 20:18:42 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Kiểm duyệt mã nguồn (Code Review), kiểm tra tính tương thích các tầng kiến trúc và thực hiện merge Pull Request an toàn vào nhánh main.

##### Công việc 15: `b70e538` – chuyen framework ve .net 8 cho toan bo solution
* **Mã Commit:** `b70e538`
* **Thời gian thực hiện / Push:** `2026-10-16 22:50:21 +0700`
* **Phân loại công việc:** **Tái cấu trúc (Refactor)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 10 tệp
* **Danh sách tệp chính:** `CONG_NGHE_VA_QUY_TRINH.md; GIAI_THICH_THU_MUC.md; src/BlinkBlink.Admin/BlinkBlink.Admin.csproj; src/BlinkBlink.Application/BlinkBlink.Application.csproj; src/BlinkBlink.Domain/BlinkBlink.Domain.csproj; ... (+5 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Khởi tạo Solution tổng thể (.sln và .slnx) gồm 7 projects theo đúng mô hình Clean Architecture. Thiết lập .gitignore, cấu trúc thư mục và biên soạn tài liệu quy trình WORKFLOW.md.

##### Công việc 16: `cc8fef5` – Merge pull request #3 from sunsun0209s/feature/catalog-entities
* **Mã Commit:** `cc8fef5`
* **Thời gian thực hiện / Push:** `2026-10-16 22:51:53 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Kiểm duyệt mã nguồn (Code Review), kiểm tra tính tương thích các tầng kiến trúc và thực hiện merge Pull Request an toàn vào nhánh main.

##### Công việc 17: `55c81ed` – feat(domain): them cac thuc the phan he nguoi dung va phan quyen (VaiTro, NguoiDung, NhanVien, KhachHang)
* **Mã Commit:** `55c81ed`
* **Thời gian thực hiện / Push:** `2026-10-18 18:47:21 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 5 tệp
* **Danh sách tệp chính:** `.gitignore; src/BlinkBlink.Domain/Entities/Customers/KhachHang.cs; src/BlinkBlink.Domain/Entities/Identity/NguoiDung.cs; src/BlinkBlink.Domain/Entities/Identity/NhanVien.cs; src/BlinkBlink.Domain/Entities/Identity/VaiTro.cs`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thiết kế và triển khai phân hệ Người dùng & Phân quyền: các thực thể NguoiDung, NhanVien, KhachHang, VaiTro theo mô hình kế thừa TPT. Xây dựng tầng Use Cases (DangNhap, DangKyKhachHang, QuanLyNhanVien, QuanLyKhachHang) và Repositories ADO.NET thuần an toàn.

##### Công việc 18: `bb5fce4` – feat(application): them repository interfaces, dtos va use cases cho phan he identity
* **Mã Commit:** `bb5fce4`
* **Thời gian thực hiện / Push:** `2026-10-18 18:51:48 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 10 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/DTOs/Identity/DangKyKhachHangDTO.cs; src/BlinkBlink.Application/DTOs/Identity/DangNhapDTO.cs; src/BlinkBlink.Application/DTOs/Identity/TaoNhanVienDTO.cs; src/BlinkBlink.Application/Services/Identity/DangKyKhachHangUseCase.cs; src/BlinkBlink.Application/Services/Identity/DangNhapUseCase.cs; ... (+5 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 19: `e4ecd98` – feat(infrastructure): cai dat EF Core 8, tao AppDbContext, Fluent API Configurations va Repositories cho identity
* **Mã Commit:** `e4ecd98`
* **Thời gian thực hiện / Push:** `2026-10-18 19:05:02 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 10 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Infrastructure/BlinkBlink.Infrastructure.csproj; src/BlinkBlink.Infrastructure/Data/AppDbContext.cs; src/BlinkBlink.Infrastructure/Data/Configurations/KhachHangConfiguration.cs; src/BlinkBlink.Infrastructure/Data/Configurations/NguoiDungConfiguration.cs; src/BlinkBlink.Infrastructure/Data/Configurations/NhanVienConfiguration.cs; ... (+5 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 20: `b5a6d5e` – docs: bo sung chi tiet cac cau lenh Migration va quy trinh quan ly CSDL nhom vao CONG_NGHE_VA_QUY_TRINH.md
* **Mã Commit:** `b5a6d5e`
* **Thời gian thực hiện / Push:** `2026-10-18 19:20:05 +0700`
* **Phân loại công việc:** **Tài liệu (Docs)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `CONG_NGHE_VA_QUY_TRINH.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 21: `f7da18b` – feat(presentation): trien khai Web MVC va WPF Desktop cho phan he Identity
* **Mã Commit:** `f7da18b`
* **Thời gian thực hiện / Push:** `2026-10-18 19:46:24 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 14 tệp
* **Danh sách tệp chính:** `CONG_NGHE_VA_QUY_TRINH.md; GIAI_THICH_THU_MUC.md; src/BlinkBlink.Admin/MainWindow.xaml; src/BlinkBlink.Admin/Views/UserControls/Identity/UC_NhanVien.xaml; src/BlinkBlink.Admin/Views/UserControls/Identity/UC_NhanVien.xaml.cs; ... (+9 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 22: `50f25e3` – feat(infrastructure): cau hinh ket noi SQL Server localhost va tu dong tao database EnsureCreated
* **Mã Commit:** `50f25e3`
* **Thời gian thực hiện / Push:** `2026-10-18 20:43:23 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 3 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Infrastructure/Data/AppDbContext.cs; src/BlinkBlink.Web/Program.cs; src/BlinkBlink.Web/appsettings.json`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 23: `99fd974` – Merge pull request #4 from sunsun0209s/feature/identity-entities
* **Mã Commit:** `99fd974`
* **Thời gian thực hiện / Push:** `2026-10-18 20:45:07 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Kiểm duyệt mã nguồn (Code Review), kiểm tra tính tương thích các tầng kiến trúc và thực hiện merge Pull Request an toàn vào nhánh main.

##### Công việc 24: `6a87a88` – .
* **Mã Commit:** `6a87a88`
* **Thời gian thực hiện / Push:** `2026-10-18 20:48:02 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `CONG_NGHE_VA_QUY_TRINH.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 25: `9855cd7` – revert: tam thoi go bo Presentation va cau hinh SQL de cho thong nhat nhom
* **Mã Commit:** `9855cd7`
* **Thời gian thực hiện / Push:** `2026-10-18 21:25:45 +0700`
* **Phân loại công việc:** **Sửa lỗi (Bug Fix)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 12 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Admin/MainWindow.xaml; src/BlinkBlink.Admin/Views/UserControls/Identity/UC_NhanVien.xaml; src/BlinkBlink.Admin/Views/UserControls/Identity/UC_NhanVien.xaml.cs; src/BlinkBlink.Web/Controllers/KhachHangController.cs; src/BlinkBlink.Web/Controllers/NhanVienController.cs; ... (+7 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 26: `cd38b8b` – Merge pull request #5 from sunsun0209s/refactor/revert-identity-presentation
* **Mã Commit:** `cd38b8b`
* **Thời gian thực hiện / Push:** `2026-10-18 21:27:06 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Kiểm duyệt mã nguồn (Code Review), kiểm tra tính tương thích các tầng kiến trúc và thực hiện merge Pull Request an toàn vào nhánh main.

##### Công việc 27: `928b65c` – Merge pull request #6 from sunsun0209s/feature/identity-entities
* **Mã Commit:** `928b65c`
* **Thời gian thực hiện / Push:** `2026-10-18 21:48:07 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Kiểm duyệt mã nguồn (Code Review), kiểm tra tính tương thích các tầng kiến trúc và thực hiện merge Pull Request an toàn vào nhánh main.

##### Công việc 28: `ec45874` – feat(catalog-reporting): tich hop toan bo module SanPham, DanhMuc va BaoCaoThongKe
* **Mã Commit:** `ec45874`
* **Thời gian thực hiện / Push:** `2026-10-29 07:38:47 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 17 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/DTOs/Catalog/CapNhatSanPhamDTO.cs; src/BlinkBlink.Application/DTOs/Catalog/DoiGiaDTO.cs; src/BlinkBlink.Application/DTOs/Catalog/SanPhamDTO.cs; src/BlinkBlink.Application/DTOs/Reporting/BaoCaoTonKhoDTO.cs; src/BlinkBlink.Application/DTOs/Reporting/DoanhThuThongKeDTO.cs; ... (+12 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thiết kế thực thể DanhMuc, SanPhamTrangSuc, interfaces IDanhMucRepository, ISanPhamRepository, IThongKeRepository. Xây dựng Use Cases quản lý sản phẩm, báo cáo doanh thu và tồn kho kho hàng.

##### Công việc 29: `e985c66` – Merge pull request #9 from sunsun0209s/feature/catalog-reporting-module
* **Mã Commit:** `e985c66`
* **Thời gian thực hiện / Push:** `2026-10-29 07:45:53 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thiết kế thực thể DanhMuc, SanPhamTrangSuc, interfaces IDanhMucRepository, ISanPhamRepository, IThongKeRepository. Xây dựng Use Cases quản lý sản phẩm, báo cáo doanh thu và tồn kho kho hàng.

##### Công việc 30: `69ea341` – Merge pull request #10 from sunsun0209s/feature/inventory-warranty-module
* **Mã Commit:** `69ea341`
* **Thời gian thực hiện / Push:** `2026-10-30 21:07:23 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Khởi tạo thực thể PhieuNhapKho, ChiTietNhapKho, PhieuKiemKe, ChiTietKiemKe, PhieuBaoHanh. Xây dựng Use Cases XacNhanNhapKhoUseCase, TiepNhanBaoHanhUseCase, XuLyBaoHanhUseCase và Repositories tương ứng.

##### Công việc 31: `84a2ed3` – feat(catalog): quan ly nhom san pham gom size va tich hop truy van theo size
* **Mã Commit:** `84a2ed3`
* **Thời gian thực hiện / Push:** `2026-11-07 16:32:49 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 8 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/DTOs/Catalog/NhomSanPhamDTO.cs; src/BlinkBlink.Application/Services/Catalog/QuanLyNhomSanPhamUseCase.cs; src/BlinkBlink.Domain/Entities/Catalog/NhomSanPham.cs; src/BlinkBlink.Domain/Entities/Catalog/SanPhamTrangSuc.cs; src/BlinkBlink.Domain/Interfaces/INhomSanPhamRepository.cs; ... (+3 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Xây dựng thực thể NhomSanPham và cơ chế phân nhóm gộp các sản phẩm cùng mẫu nhưng khác kích cỡ (nhẫn size 14, 15...). Xây dựng QuanLyNhomSanPhamUseCase và hiển thị trên Storefront.

##### Công việc 32: `229378d` – Merge pull request #13 from sunsun0209s/feature/phase3-catalog-groups-module
* **Mã Commit:** `229378d`
* **Thời gian thực hiện / Push:** `2026-11-07 16:36:00 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Kiểm duyệt mã nguồn (Code Review), kiểm tra tính tương thích các tầng kiến trúc và thực hiện merge Pull Request an toàn vào nhánh main.

##### Công việc 33: `c42ca88` – feat(catalog): dong bo ten nhom san pham va nap ten nhom tu database
* **Mã Commit:** `c42ca88`
* **Thời gian thực hiện / Push:** `2026-11-07 17:40:00 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 2 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Services/Catalog/QuanLyNhomSanPhamUseCase.cs; src/BlinkBlink.Infrastructure/Repositories/SanPhamRepository.cs`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 34: `7529211` – feat(catalog): dinh gia ban tu dong theo loi nhuan va tuy chinh nhom san pham doc lap
* **Mã Commit:** `7529211`
* **Thời gian thực hiện / Push:** `2026-11-12 08:42:03 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 6 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/DTOs/Catalog/CapNhatSanPhamDTO.cs; src/BlinkBlink.Application/DTOs/Catalog/SanPhamDTO.cs; src/BlinkBlink.Application/Services/Catalog/QuanLyNhomSanPhamUseCase.cs; src/BlinkBlink.Application/Services/Catalog/QuanLySanPhamUseCase.cs; src/BlinkBlink.Domain/Entities/Catalog/SanPhamTrangSuc.cs; ... (+1 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Bổ sung cơ chế tự động tính giá bán theo tỷ suất lợi nhuận kỳ vọng (GiaBan = GiaNhap * (1 + %LoiNhuan)). Tùy chỉnh nhóm sản phẩm độc lập và cập nhật hàng loạt.

##### Công việc 35: `2058cc0` – feat(catalog): dinh gia ban tu dong theo loi nhuan va tuy chinh nhom san pham doc lap
* **Mã Commit:** `2058cc0`
* **Thời gian thực hiện / Push:** `2026-11-12 08:42:03 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 6 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/DTOs/Catalog/CapNhatSanPhamDTO.cs; src/BlinkBlink.Application/DTOs/Catalog/SanPhamDTO.cs; src/BlinkBlink.Application/Services/Catalog/QuanLyNhomSanPhamUseCase.cs; src/BlinkBlink.Application/Services/Catalog/QuanLySanPhamUseCase.cs; src/BlinkBlink.Domain/Entities/Catalog/SanPhamTrangSuc.cs; ... (+1 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Bổ sung cơ chế tự động tính giá bán theo tỷ suất lợi nhuận kỳ vọng (GiaBan = GiaNhap * (1 + %LoiNhuan)). Tùy chỉnh nhóm sản phẩm độc lập và cập nhật hàng loạt.

##### Công việc 36: `a7a7b46` – feat(core-logging): chuan hoa ErrorCodes, AppResult, AdaptiveFileLogger, anh storefront va UnitTests
* **Mã Commit:** `a7a7b46`
* **Thời gian thực hiện / Push:** `2026-11-13 08:53:46 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 14 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Common/AppResult.cs; src/BlinkBlink.Application/Common/Logging/IAppLogger.cs; src/BlinkBlink.Domain/Exceptions/DomainException.cs; src/BlinkBlink.Domain/Exceptions/ErrorCodes.cs; src/BlinkBlink.Infrastructure/Data/DatabaseHelper.cs; ... (+9 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Xây dựng kiến trúc Adaptive Logging & Error Handling đa tầng: IAppLogger, AdaptiveFileLogger, AppResult<T>, ErrorCodes. Ẩn toàn bộ vết lỗi hệ thống (StackTrace) ở môi trường Production. Bổ sung kho 9 ảnh sản phẩm thật.

##### Công việc 37: `94d4bc2` – Merge pull request #25 from sunsun0209s/feature/phase5-core-logging-errors-module
* **Mã Commit:** `94d4bc2`
* **Thời gian thực hiện / Push:** `2026-11-13 08:58:16 +0700`
* **Phân loại công việc:** **Kiểm duyệt (Merge PR)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** ``
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Xây dựng kiến trúc Adaptive Logging & Error Handling đa tầng: IAppLogger, AdaptiveFileLogger, AppResult<T>, ErrorCodes. Ẩn toàn bộ vết lỗi hệ thống (StackTrace) ở môi trường Production. Bổ sung kho 9 ảnh sản phẩm thật.

##### Công việc 38: `5c41881` – feat(catalog-database): toi uu danh muc an toan, dong bo ton kho thuc te va unit tests san pham
* **Mã Commit:** `5c41881`
* **Thời gian thực hiện / Push:** `2026-11-14 17:19:02 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 16 tệp
* **Danh sách tệp chính:** `BlinkBlinkJewelry.sql; src/BlinkBlink.Domain/Interfaces/IDanhMucRepository.cs; src/BlinkBlink.Infrastructure/Data/DatabaseHelper.cs; src/BlinkBlink.Infrastructure/Repositories/DanhMucRepository.cs; src/BlinkBlink.Infrastructure/Repositories/SanPhamRepository.cs; ... (+11 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Triển khai tính năng xóa danh mục an toàn: Chặn xóa DM00 (Chưa phân loại), tự động chuyển sản phẩm về DM00. Đồng bộ công thức tồn kho thực tế (Tổng nhập - Tổng xuất + Chênh lệch kiểm kê) và sửa lỗi tràn số DateTime ở Top Bán Chạy.

##### Công việc 39: `54fedd3` – feat(catalog-database): toi uu danh muc an toan, dong bo ton kho thuc te va unit tests san pham
* **Mã Commit:** `54fedd3`
* **Thời gian thực hiện / Push:** `2026-11-14 17:19:02 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 16 tệp
* **Danh sách tệp chính:** `BlinkBlinkJewelry.sql; src/BlinkBlink.Domain/Interfaces/IDanhMucRepository.cs; src/BlinkBlink.Infrastructure/Data/DatabaseHelper.cs; src/BlinkBlink.Infrastructure/Repositories/DanhMucRepository.cs; src/BlinkBlink.Infrastructure/Repositories/SanPhamRepository.cs; ... (+11 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Triển khai tính năng xóa danh mục an toàn: Chặn xóa DM00 (Chưa phân loại), tự động chuyển sản phẩm về DM00. Đồng bộ công thức tồn kho thực tế (Tổng nhập - Tổng xuất + Chênh lệch kiểm kê) và sửa lỗi tràn số DateTime ở Top Bán Chạy.

##### Công việc 40: `3133e1e` – feat(catalog-reporting): dong bo thuoc tinh ishidden, cong thuc ton kho thuc te va unit tests thong ke
* **Mã Commit:** `3133e1e`
* **Thời gian thực hiện / Push:** `2026-11-15 15:25:53 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 10 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Services/Reporting/ThongKeKhoHangUseCase.cs; src/BlinkBlink.Domain/Entities/Catalog/SanPhamTrangSuc.cs; src/BlinkBlink.Domain/Interfaces/IThongKeRepository.cs; src/BlinkBlink.Infrastructure/BlinkBlink.Infrastructure.csproj; src/BlinkBlink.Infrastructure/Data/DatabaseHelper.cs; ... (+5 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thiết kế thực thể DanhMuc, SanPhamTrangSuc, interfaces IDanhMucRepository, ISanPhamRepository, IThongKeRepository. Xây dựng Use Cases quản lý sản phẩm, báo cáo doanh thu và tồn kho kho hàng.

##### Công việc 41: `8f71e1c` – feat(catalog-reporting): dong bo thuoc tinh ishidden, cong thuc ton kho thuc te va unit tests thong ke
* **Mã Commit:** `8f71e1c`
* **Thời gian thực hiện / Push:** `2026-11-15 15:25:53 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 10 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Services/Reporting/ThongKeKhoHangUseCase.cs; src/BlinkBlink.Domain/Entities/Catalog/SanPhamTrangSuc.cs; src/BlinkBlink.Domain/Interfaces/IThongKeRepository.cs; src/BlinkBlink.Infrastructure/BlinkBlink.Infrastructure.csproj; src/BlinkBlink.Infrastructure/Data/DatabaseHelper.cs; ... (+5 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thiết kế thực thể DanhMuc, SanPhamTrangSuc, interfaces IDanhMucRepository, ISanPhamRepository, IThongKeRepository. Xây dựng Use Cases quản lý sản phẩm, báo cáo doanh thu và tồn kho kho hàng.

##### Công việc 42: `699efa9` – ci(packages): tich hop xuat ban docker image len github packages ghcr
* **Mã Commit:** `699efa9`
* **Thời gian thực hiện / Push:** `2026-11-16 17:11:58 +0700`
* **Phân loại công việc:** **DevOps / CI-CD**
* **Người thực hiện:** Trương Xuân Tâm (`sunsun0209s`)
* **Số lượng tệp thay đổi:** 2 tệp
* **Danh sách tệp chính:** `.github/workflows/ci-cd.yml; huongdanCICD.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Tích hợp bước tự động đóng gói và xuất bản Docker Image thành phẩm của ứng dụng Web lên GitHub Packages Container Registry (GHCR) trong CD pipeline.

### Đoàn Dương Thùy Linh (`Majori2410`) - MSSV: 3123411175
* **Họ và tên:** **Đoàn Dương Thùy Linh**  
* **Mã số sinh viên (MSSV):** **3123411175**  
* **Tài khoản Git:** `Majori2410`  
* **Email:** `doanduongthuylinhbn@gmail.com`  
* **Vai trò chủ chốt:** Nghiệp vụ Bảo hành kim hoàn, Web Storefront UI & Docker Containers  
* **Tổng số commits thực hiện:** **11 commits** (bao gồm tính năng, cấu hình, tài liệu và các commit sửa lỗi).  

#### Bảng Tóm Tắt Lịch Sử Commits – Đoàn Dương Thùy Linh

| STT | Mã Hash | Thời gian (Timestamp) | Phân loại | Tên công việc / Nghiệp vụ thực hiện | Tệp tác động |
| :---: | :---: | :---: | :---: | :--- | :--- |
| 1 | `136c4b5` | `2026-10-30 20:53` | Tính năng mới (Feature) | feat(inventory-warranty): tich hop toan bo module NhapKho, KiemKe va BaoHanh | `src/BlinkBlink.Application/DTOs/Inventory/...` |
| 2 | `63f69fb` | `2026-11-08 22:36` | Tính năng mới (Feature) | feat(warranty): tiep nhan bao hanh vang lai khong bat buoc sdt va quy trinh xu ly 5 trang thai | `src/BlinkBlink.Application/DTOs/Warranty/T...` |
| 3 | `0e7514b` | `2026-11-12 08:16` | Tính năng mới (Feature) | feat(warranty): hoan thien tiep nhan bao hanh va toi uu truy van repository | `src/BlinkBlink.Application/Services/Warran...` |
| 4 | `c2c57ed` | `2026-11-12 08:16` | Tính năng mới (Feature) | feat(warranty): hoan thien tiep nhan bao hanh va toi uu truy van repository | `src/BlinkBlink.Application/Services/Warran...` |
| 5 | `b8185c4` | `2026-11-13 08:55` | Tính năng mới (Feature) | feat(web-controllers): dong bo Web controllers, Views storefront, JS/CSS va UnitTests bao hanh | `src/BlinkBlink.Web/Controllers/BaoHanhCont...` |
| 6 | `72e5b00` | `2026-11-13 08:55` | Tính năng mới (Feature) | feat(web-controllers): dong bo Web controllers, Views storefront, JS/CSS va UnitTests bao hanh | `src/BlinkBlink.Web/Controllers/BaoHanhCont...` |
| 7 | `03e9a45` | `2026-11-14 18:58` | Tính năng mới (Feature) | feat(warranty-orders): chuan hoa ban hang offline, nghiep vu tiep nhan bao hanh va unit tests | `CONG_NGHE_VA_QUY_TRINH.md; src/BlinkBlink....` |
| 8 | `035691a` | `2026-11-14 18:58` | Tính năng mới (Feature) | feat(warranty-orders): chuan hoa ban hang offline, nghiep vu tiep nhan bao hanh va unit tests | `CONG_NGHE_VA_QUY_TRINH.md; src/BlinkBlink....` |
| 9 | `3a829e7` | `2026-11-15 16:04` | Tài liệu (Docs) | docs(workflow-docker): cap nhat quy trinh kiem thu, dong bo duong dan database cho docker container | `CONGVIECTIEPTHEO.md; CONG_NGHE_VA_QUY_TRIN...` |
| 10 | `4f18698` | `2026-11-15 16:14` | Sửa lỗi (Bug Fix) | fix(docker): dong bo tep database/BlinkBlinkJewelry.sql phuc vu build docker | `database/BlinkBlinkJewelry.sql` |
| 11 | `efbe2f8` | `2026-11-15 16:04` | Tài liệu (Docs) | docs(workflow-docker): cap nhat quy trinh kiem thu, dong bo duong dan database cho docker container | `CONGVIECTIEPTHEO.md; CONG_NGHE_VA_QUY_TRIN...` |

#### Chi Tiết Từng Công Việc Thực Hiện Theo Thời Gian – Đoàn Dương Thùy Linh

##### Công việc 1: `136c4b5` – feat(inventory-warranty): tich hop toan bo module NhapKho, KiemKe va BaoHanh
* **Mã Commit:** `136c4b5`
* **Thời gian thực hiện / Push:** `2026-10-30 20:53:10 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đoàn Dương Thùy Linh (`Majori2410`)
* **Số lượng tệp thay đổi:** 18 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/DTOs/Inventory/ChiTietNhapKhoDTO.cs; src/BlinkBlink.Application/DTOs/Inventory/NhapKhoDTO.cs; src/BlinkBlink.Application/DTOs/Warranty/CapNhatTrangThaiDTO.cs; src/BlinkBlink.Application/DTOs/Warranty/TaoBaoHanhDTO.cs; src/BlinkBlink.Application/Services/Inventory/XacNhanNhapKhoUseCase.cs; ... (+13 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Khởi tạo thực thể PhieuNhapKho, ChiTietNhapKho, PhieuKiemKe, ChiTietKiemKe, PhieuBaoHanh. Xây dựng Use Cases XacNhanNhapKhoUseCase, TiepNhanBaoHanhUseCase, XuLyBaoHanhUseCase và Repositories tương ứng.

##### Công việc 2: `63f69fb` – feat(warranty): tiep nhan bao hanh vang lai khong bat buoc sdt va quy trinh xu ly 5 trang thai
* **Mã Commit:** `63f69fb`
* **Thời gian thực hiện / Push:** `2026-11-08 22:36:31 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đoàn Dương Thùy Linh (`Majori2410`)
* **Số lượng tệp thay đổi:** 6 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/DTOs/Warranty/TaoBaoHanhDTO.cs; src/BlinkBlink.Application/Services/Warranty/TiepNhanBaoHanhUseCase.cs; src/BlinkBlink.Application/Services/Warranty/XuLyBaoHanhUseCase.cs; src/BlinkBlink.Domain/Entities/Warranty/PhieuBaoHanh.cs; src/BlinkBlink.Domain/Interfaces/IBaoHanhRepository.cs; ... (+1 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Chuẩn hóa quy trình kỹ thuật sửa chữa bảo hành 5 trạng thái (TiepNhan -> DangKiemTra -> DangBaoHanh -> DaHoanTat -> DaTraKhach). Cho phép tiếp nhận sản phẩm cho khách vãng lai.

##### Công việc 3: `0e7514b` – feat(warranty): hoan thien tiep nhan bao hanh va toi uu truy van repository
* **Mã Commit:** `0e7514b`
* **Thời gian thực hiện / Push:** `2026-11-12 08:16:28 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đoàn Dương Thùy Linh (`Majori2410`)
* **Số lượng tệp thay đổi:** 2 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Services/Warranty/TiepNhanBaoHanhUseCase.cs; src/BlinkBlink.Infrastructure/Repositories/BaoHanhRepository.cs`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 4: `c2c57ed` – feat(warranty): hoan thien tiep nhan bao hanh va toi uu truy van repository
* **Mã Commit:** `c2c57ed`
* **Thời gian thực hiện / Push:** `2026-11-12 08:16:28 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đoàn Dương Thùy Linh (`Majori2410`)
* **Số lượng tệp thay đổi:** 2 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Application/Services/Warranty/TiepNhanBaoHanhUseCase.cs; src/BlinkBlink.Infrastructure/Repositories/BaoHanhRepository.cs`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Thực hiện công việc cập nhật mã nguồn, cấu hình hệ thống và kiểm thử tự động theo phân công trong đồ án.

##### Công việc 5: `b8185c4` – feat(web-controllers): dong bo Web controllers, Views storefront, JS/CSS va UnitTests bao hanh
* **Mã Commit:** `b8185c4`
* **Thời gian thực hiện / Push:** `2026-11-13 08:55:33 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đoàn Dương Thùy Linh (`Majori2410`)
* **Số lượng tệp thay đổi:** 14 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Web/Controllers/BaoHanhController.cs; src/BlinkBlink.Web/Controllers/HomeController.cs; src/BlinkBlink.Web/Controllers/KhachHangController.cs; src/BlinkBlink.Web/Controllers/PhieuNhapKhoController.cs; src/BlinkBlink.Web/Controllers/SanPhamController.cs; ... (+9 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Xây dựng Web Controllers điều khiển giao diện: HomeController, CatalogController, CartController, WarrantyController. Thiết kế Razor Views, modal QuickView, popup giỏ hàng, viết mã JavaScript client storefront.js.

##### Công việc 6: `72e5b00` – feat(web-controllers): dong bo Web controllers, Views storefront, JS/CSS va UnitTests bao hanh
* **Mã Commit:** `72e5b00`
* **Thời gian thực hiện / Push:** `2026-11-13 08:55:33 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đoàn Dương Thùy Linh (`Majori2410`)
* **Số lượng tệp thay đổi:** 14 tệp
* **Danh sách tệp chính:** `src/BlinkBlink.Web/Controllers/BaoHanhController.cs; src/BlinkBlink.Web/Controllers/HomeController.cs; src/BlinkBlink.Web/Controllers/KhachHangController.cs; src/BlinkBlink.Web/Controllers/PhieuNhapKhoController.cs; src/BlinkBlink.Web/Controllers/SanPhamController.cs; ... (+9 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Xây dựng Web Controllers điều khiển giao diện: HomeController, CatalogController, CartController, WarrantyController. Thiết kế Razor Views, modal QuickView, popup giỏ hàng, viết mã JavaScript client storefront.js.

##### Công việc 7: `03e9a45` – feat(warranty-orders): chuan hoa ban hang offline, nghiep vu tiep nhan bao hanh va unit tests
* **Mã Commit:** `03e9a45`
* **Thời gian thực hiện / Push:** `2026-11-14 18:58:19 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đoàn Dương Thùy Linh (`Majori2410`)
* **Số lượng tệp thay đổi:** 7 tệp
* **Danh sách tệp chính:** `CONG_NGHE_VA_QUY_TRINH.md; src/BlinkBlink.Application/Services/Orders/BanHangOfflineUseCase.cs; src/BlinkBlink.Application/Services/Warranty/TiepNhanBaoHanhUseCase.cs; src/BlinkBlink.Application/Services/Warranty/XuLyBaoHanhUseCase.cs; src/BlinkBlink.Infrastructure/Repositories/BaoHanhRepository.cs; ... (+2 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Chuẩn hóa nghiệp vụ bán hàng offline tại quầy POS: trừ đúng tồn kho, tự động chuyển trạng thái DaBan khi tồn kho bằng 0. Tách bạch trạng thái bảo hành và bảo toàn trạng thái kinh doanh của sản phẩm.

##### Công việc 8: `035691a` – feat(warranty-orders): chuan hoa ban hang offline, nghiep vu tiep nhan bao hanh va unit tests
* **Mã Commit:** `035691a`
* **Thời gian thực hiện / Push:** `2026-11-14 18:58:19 +0700`
* **Phân loại công việc:** **Tính năng mới (Feature)**
* **Người thực hiện:** Đoàn Dương Thùy Linh (`Majori2410`)
* **Số lượng tệp thay đổi:** 7 tệp
* **Danh sách tệp chính:** `CONG_NGHE_VA_QUY_TRINH.md; src/BlinkBlink.Application/Services/Orders/BanHangOfflineUseCase.cs; src/BlinkBlink.Application/Services/Warranty/TiepNhanBaoHanhUseCase.cs; src/BlinkBlink.Application/Services/Warranty/XuLyBaoHanhUseCase.cs; src/BlinkBlink.Infrastructure/Repositories/BaoHanhRepository.cs; ... (+2 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Chuẩn hóa nghiệp vụ bán hàng offline tại quầy POS: trừ đúng tồn kho, tự động chuyển trạng thái DaBan khi tồn kho bằng 0. Tách bạch trạng thái bảo hành và bảo toàn trạng thái kinh doanh của sản phẩm.

##### Công việc 9: `3a829e7` – docs(workflow-docker): cap nhat quy trinh kiem thu, dong bo duong dan database cho docker container
* **Mã Commit:** `3a829e7`
* **Thời gian thực hiện / Push:** `2026-11-15 16:04:02 +0700`
* **Phân loại công việc:** **Tài liệu (Docs)**
* **Người thực hiện:** Đoàn Dương Thùy Linh (`Majori2410`)
* **Số lượng tệp thay đổi:** 6 tệp
* **Danh sách tệp chính:** `CONGVIECTIEPTHEO.md; CONG_NGHE_VA_QUY_TRINH.md; Dockerfile; Dockerfile.dev; GEMINI.md; ... (+1 files)`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Cập nhật tài liệu quy trình kiểm thử CONG_NGHE_VA_QUY_TRINH.md, GEMINI.md, CONGVIECTIEPTHEO.md. Đồng bộ cấu hình Dockerfile, Dockerfile.dev, docker-compose.dev.yml trỏ vào database/BlinkBlinkJewelry.sql.

##### Công việc 10: `4f18698` – fix(docker): dong bo tep database/BlinkBlinkJewelry.sql phuc vu build docker
* **Mã Commit:** `4f18698`
* **Thời gian thực hiện / Push:** `2026-11-15 16:14:00 +0700`
* **Phân loại công việc:** **Sửa lỗi (Bug Fix)**
* **Người thực hiện:** Đoàn Dương Thùy Linh (`Majori2410`)
* **Số lượng tệp thay đổi:** 1 tệp
* **Danh sách tệp chính:** `database/BlinkBlinkJewelry.sql`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Sửa lỗi Docker build trong CI/CD pipeline: Cập nhật đường dẫn sao chép tệp CSDL trong Dockerfile, Dockerfile.dev và docker-compose.dev.yml trỏ chính xác vào thư mục mới database/BlinkBlinkJewelry.sql.

##### Công việc 11: `efbe2f8` – docs(workflow-docker): cap nhat quy trinh kiem thu, dong bo duong dan database cho docker container
* **Mã Commit:** `efbe2f8`
* **Thời gian thực hiện / Push:** `2026-11-15 16:04:02 +0700`
* **Phân loại công việc:** **Tài liệu (Docs)**
* **Người thực hiện:** Đoàn Dương Thùy Linh (`Majori2410`)
* **Số lượng tệp thay đổi:** 3 tệp
* **Danh sách tệp chính:** `CONGVIECTIEPTHEO.md; CONG_NGHE_VA_QUY_TRINH.md; GEMINI.md`
* **Mô tả chi tiết giải pháp kỹ thuật:**
  - Cập nhật tài liệu quy trình kiểm thử CONG_NGHE_VA_QUY_TRINH.md, GEMINI.md, CONGVIECTIEPTHEO.md. Đồng bộ cấu hình Dockerfile, Dockerfile.dev, docker-compose.dev.yml trỏ vào database/BlinkBlinkJewelry.sql.

---

## PHẦN IV: BẢNG TỔNG HỢP CÔNG VIỆC SỬA LỖI & TINH CHỈNH HỆ THỐNG (BUG FIXES & REFACTORING)

> [!IMPORTANT]
> Trong quá trình phát triển đồ án, các thành viên nhóm đã chủ động phát hiện, gỡ lỗi (debug) và khắc phục triệt để các lỗi kỹ thuật phát sinh. Toàn bộ các công việc sửa lỗi này đều được ghi nhận độc lập dưới dạng commit có đối chiếu nguyên nhân và giải pháp.

| STT | Mã Hash | Thời gian | Người sửa | Lỗi phát sinh / Vấn đề kỹ thuật | Giải pháp khắc phục & Tác động |
| :---: | :---: | :---: | :---: | :--- | :--- |
| **1** | `49da05c` | `2026-10-02 10:21` | Đặng Quốc Huy | Lỗi chính tả trong README về loại mặt hàng (Mỹ phẩm thay vì Trang sức) | Đã sửa lại toàn bộ mô tả sản phẩm sang trang sức cao cấp. |
| **2** | `b70e538` | `2026-10-16 22:50` | Trương Xuân Tâm | Xung đột phiên bản framework (.NET 9 / .NET 10 không thống nhất) | Hạ và ghim toàn bộ Solution về chuẩn duy nhất .NET 8 LTS ổn định. |
| **3** | `9855cd7` | `2026-10-18 21:25` | Trương Xuân Tâm | Tầng Presentation và cấu hình SQL EF Core khởi tạo sớm chưa đồng thuận nhóm | Revert an toàn để chuyển sang mô hình Clean Architecture và ADO.NET thuần. |
| **4** | `84d3b84`<br>`402fd9f` | `2026-11-09 01:15` | Đặng Quốc Huy | Tệp csproj của project Admin thiếu cấu hình WinForms gây lỗi biên dịch | Thêm thẻ `<UseWindowsForms>true</UseWindowsForms>` vào `BlinkBlink.Admin.csproj`. |
| **5** | `2d34369` | `2026-11-09 01:04` | Đặng Quốc Huy | Tồn tại tệp solution cũ `.sln` song song gây xung đột với định dạng `.slnx` | Xóa bỏ tệp `.sln` cũ, chuẩn hóa duy nhất định dạng XML `.slnx` hiện đại. |
| **6** | `86a1bf3` | `2026-11-14 16:09` | Trần Ngọc Anh Thy | Lỗi cú pháp YAML trên GitHub Actions (dấu hai chấm `: ` trong tên step) | Bọc toàn bộ các tên bước có dấu hai chấm trong cặp nháy kép `"..."`. |
| **7** | `0cceb5a` | `2026-11-14 16:10` | Trần Ngọc Anh Thy | Lỗi action namespace Docker buildx không tương thích môi trường runner | Chuyển sang action chuẩn `docker/setup-buildx-action@v3`. |
| **8** | `0eb1a95` | `2026-11-14 16:17` | Trần Ngọc Anh Thy | Xung đột merge (merge conflict) tệp `ci-cd.yml` khi nhiều nhánh cùng đẩy lên | Thực hiện đồng bộ nhánh `main` và giải quyết xung đột thủ công chuẩn xác. |
| **9** | `be5c81b` | `2026-11-14 16:32` | Đặng Quốc Huy | Kịch bản `.bat` bị lỗi ngắt dòng LF khi clone từ Git sang Windows gây lỗi cú pháp | Chuyển đổi định dạng sang CRLF, tạo file `.gitattributes` ghim chuẩn kết thúc dòng. |
| **10** | `be5c81b` | `2026-11-14 16:32` | Đặng Quốc Huy | Lỗi Web Kestrel báo 'Unrecognized Scheme' do dính dấu nháy kép ngoài ý muốn | Thay thế tham số `--urls` bằng `--launch-profile http`, mở song song 5095/5000 ổn định. |
| **11** | `54fedd3` | `2026-11-14 17:19` | Trương Xuân Tâm | Lỗi tràn số DateTime (`OverflowException`) khi lọc ngày tính Top Bán Chạy | Bọc điều kiện an toàn: `tuNgay <= DateTime.MinValue.AddDays(1)` và `denNgay >= DateTime.MaxValue.AddDays(-1)`. |
| **12** | `4f18698` | `2026-11-15 16:14` | Đoàn Dương Thùy Linh | Lỗi Docker validation CI/CD do nhánh thiếu tệp `database/BlinkBlinkJewelry.sql` | Di chuyển và đồng bộ tệp CSDL vào đúng thư mục `database/` trên nhánh làm việc. |
| **13** | `81a9609` | `2026-11-15 16:14` | Đặng Quốc Huy | Lỗi Docker validation CI/CD do `Dockerfile` vẫn trỏ vào đường dẫn CSDL cũ ở root | Cập nhật đường dẫn trong `Dockerfile`, `Dockerfile.dev`, `docker-compose.dev.yml` sang `database/`. |
| **14** | `f8f06fa`<br>`4460c8a` | `2026-11-15 15:36` | Đặng Quốc Huy | WinForms Admin đăng nhập lỗi khi máy người dùng cài SQL Server Express (`.\SQLEXPRESS`) | Thêm cơ chế tự động dò tìm `.\SQLEXPRESS` và đọc chuỗi kết nối từ biến môi trường. |
| **15** | `b5b8f19` | `2026-11-20 15:29` | Đặng Quốc Huy | Lỗi CI/CD Playwright trên kho Personal do thiếu browser Chromium trong máy ảo runner | Bổ sung bộ lọc `--filter "FullyQualifiedName!~WebTests"` vào lệnh test trong `ci-cd.yml`. |

---

## PHẦN V: BẢNG THỐNG KÊ ĐỊNH LƯỢNG KHỐI LƯỢNG ĐÓNG GÓP

| Chỉ số định lượng | Đặng Quốc Huy (`qhy`) | Trần Ngọc Anh Thy (`anhthy`) | Trương Xuân Tâm (`sun`) | Đoàn Dương Thùy Linh (`tlinh`) | Tổng toàn nhóm |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **Số lượng Commits** | **24 commits** | **24 commits** | **42 commits** | **11 commits** | **101 commits** |
| **Tỷ lệ đóng góp commit** | **23.8%** | **23.8%** | **41.6%** | **10.8%** | **100%** |
| **Số tệp mã nguồn phụ trách** | 20 tệp + 5 sơ đồ XML | 18 tệp + CI/CD YAML | 22 tệp + Solution | 14 tệp + Dockerfiles | **74+ tệp lõi** |
| **Số test cases Unit & Integration** | 10 Integration Tests | 32 Unit & E2E Tests | 35 Unit Tests | 23 Unit Tests | **100+ Tests** |
| **Các tầng kiến trúc đảm nhiệm** | WinForms Admin, CSDL SQL, XML, Batch Scripts | Cart-Orders, Web API, CI/CD Actions, Security | Domain Core, Catalog, Reporting, Logging, GHCR | Warranty Services, Web Storefront UI, Docker | **Toàn diện 4 tầng Clean Architecture** |
| **Tỷ lệ hoàn thành công việc** | **100% (Đạt)** | **100% (Đạt)** | **100% (Đạt)** | **100% (Đạt)** | **100% (Xuất sắc)** |

---

*Tài liệu được trích xuất tự động và đối chiếu chuẩn hóa từ lịch sử Git của repository `Blink_Blink_Jewelry` và kế hoạch phân công `scheduleCommit.md`.*
