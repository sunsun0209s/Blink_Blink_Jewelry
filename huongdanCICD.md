# HƯỚNG DẪN VẬN HÀNH HỆ THỐNG CI/CD - BLINK BLINK JEWELRY

Tài liệu này cung cấp hướng dẫn chi tiết về cấu hình, nguyên lý hoạt động và quy trình vận hành hệ thống Tích hợp liên tục và Phân phối liên tục (CI/CD) của dự án **Blink Blink Jewelry**, được tự động hóa qua nền tảng **GitHub Actions**.

---

## 1. Tổng Quan Kiến Trúc CI/CD

Dự án Blink Blink Jewelry bao gồm kiến trúc Clean Architecture trên nền tảng .NET 8 LTS kết hợp giữa hai môi trường:
* **Giao diện Quản trị / Bán hàng POS:** Ứng dụng Windows Forms (`net8.0-windows`).
* **Giao diện Khách hàng & RESTful API:** Ứng dụng ASP.NET Core (`net8.0`), đóng gói và chạy trong Docker Linux Container (`mcr.microsoft.com/dotnet/aspnet:8.0`).

Do đó, kịch bản CI/CD (`.github/workflows/ci-cd.yml`) được thiết kế theo mô hình **Đa tầng phối hợp (Multi-Stage Heterogeneous Pipeline)**:

```
[ Push / Pull Request ]
          │
          ├───► Job 1 (CI): Build & Test toàn bộ Solution trên Windows Runner (windows-latest)
          │        ├── Restore & Build Solution ở chế độ Release
          │        └── Chạy 72 ca kiểm thử tự động (Unit Tests & Integration Tests)
          │
          ├───► Job 2 (CI): Xác thực môi trường Docker Container trên Linux Runner (ubuntu-latest)
          │        ├── Kiểm tra biên dịch Dockerfile Production cho Web & API
          │        └── Kiểm tra cú pháp kịch bản docker-compose.prod.yml & docker-compose.dev.yml
          │
          ▼
   (Khi merge vào main)
          │
          └───► Job 3 (CD): Đóng gói và phát hành thành phẩm (Release Artifacts)
                   ├── Publish gói BlinkBlink.Web (sẵn sàng đưa lên máy chủ Linux / IIS)
                   ├── Publish gói BlinkBlink.Admin (ứng dụng WinForms chạy ngay trên máy trạm)
                   └── Tải lên mục Artifacts của GitHub Actions lưu trữ trong 30 ngày
```

---

## 2. Chi Tiết Các Giai Đoạn Trong Pipeline

### Giai Đoạn 1: Tích Hợp Liên Tục (CI - Build & Test)
* **Môi trường thực thi:** Máy ảo `windows-latest` có sẵn .NET 8 SDK.
* **Thời điểm kích hoạt:** Khi có bất kỳ commit mới nào được `push` lên các nhánh (`main`, `feature/**`, `bugfix/**`, `refactor/**`) hoặc khi mở một `Pull Request` vào nhánh `main`.
* **Các bước tự động:**
  1. Tải toàn bộ mã nguồn của commit tương ứng (`actions/checkout@v4`).
  2. Cài đặt phiên bản .NET 8 SDK chuẩn (`actions/setup-dotnet@v4`).
  3. Áp dụng cơ chế lưu trữ đệm (Cache) thư viện NuGet để rút ngắn thời gian tải xuống.
  4. Thực thi lệnh khôi phục gói phụ thuộc: `dotnet restore BlinkBlinkJewelry.slnx`.
  5. Biên dịch toàn bộ Solution: `dotnet build BlinkBlinkJewelry.slnx -c Release --no-restore`. Đảm bảo cả thư viện miền lõi, nghiệp vụ, cơ sở hạ tầng, Web API và ứng dụng WinForms Admin đều biên dịch không có lỗi.
  6. Khởi chạy 72 bài kiểm thử tự động: `dotnet test BlinkBlinkJewelry.slnx -c Release --no-build`.
  7. Xuất và lưu trữ kết quả kiểm thử dạng `.trx` để nhóm có thể tải về phân tích.

### Giai Đoạn 2: Xác Thực Môi Trường Docker (CI - Container Validation)
* **Môi trường thực thi:** Máy ảo `ubuntu-latest`.
* **Thời điểm kích hoạt:** Chạy song song với Giai đoạn 1 mỗi khi có push hoặc tạo Pull Request.
* **Các bước tự động:**
  1. Kiểm tra cú pháp và khả năng đóng gói nhiều tầng (Multi-stage build) của `Dockerfile` thông qua lệnh:
     ```bash
     docker build -t blinkblink-web:ci-test -f Dockerfile .
     ```
  2. Xác thực tính toàn vẹn cấu hình của cả hai môi trường Compose:
     ```bash
     docker compose -f docker-compose.prod.yml config
     docker compose -f docker-compose.dev.yml config
     ```
  3. Ngăn chặn triệt để tình trạng lỗi thụt lề YAML hoặc đường dẫn tệp sai làm gián đoạn quá trình triển khai của người dùng.

### Giai Đoạn 3: Phân Phối Liên Tục (CD - Đóng Gói Thành Phẩm)
* **Môi trường thực thi:** Máy ảo `windows-latest`.
* **Điều kiện kích hoạt:** Chỉ tự động kích hoạt khi mã nguồn được **gộp chính thức vào nhánh `main`** và cả hai Giai đoạn 1, 2 đều đạt trạng thái thành công 100%.
* **Các bước tự động:**
  1. Đóng gói ứng dụng Web khách hàng & RESTful API:
     ```powershell
     dotnet publish src/BlinkBlink.Web/BlinkBlink.Web.csproj -c Release -o ./publish/web
     ```
  2. Đóng gói ứng dụng Quản trị WinForms Admin cho hệ điều hành Windows:
     ```powershell
     dotnet publish src/BlinkBlink.Admin/BlinkBlink.Admin.csproj -c Release -r win-x64 --self-contained false -o ./publish/admin
     ```
  3. Tải hai gói phát hành hoàn chỉnh lên mục **Artifacts** của đợt chạy:
     * `blinkblink-web-production-release`: Gói Web sẵn sàng cấu hình chạy Production.
     * `blinkblink-admin-winforms-release`: Gói phần mềm WinForms Admin sẵn sàng chạy ngay trên Windows.

---

## 3. Quy Trình Làm Việc Nhóm Với CI/CD (Quy Chuẩn Chặn Lỗi)

Để đảm bảo nhánh `main` luôn là phiên bản chạy ổn định nhất (Production-ready), nhóm cần tuân thủ quy trình sau:

1. **Làm việc trên nhánh riêng:**
   * Mỗi thành viên tạo nhánh từ `main`:
     ```bash
     git checkout main
     git pull origin main
     git checkout -b feature/<ten-tinh-nang>
     ```
2. **Commit và đẩy lên GitHub:**
   * Commit mã nguồn và đẩy nhánh lên GitHub:
     ```bash
     git push origin feature/<ten-tinh-nang>
     ```
   * Ngay khi đẩy, GitHub Actions sẽ tự động kích hoạt Job 1 và Job 2 để kiểm tra nhánh của bạn.
3. **Mở Pull Request (PR) để ghép vào `main`:**
   * Vào giao diện GitHub, tạo Pull Request từ nhánh của bạn vào `main`.
   * Giao diện GitHub sẽ hiển thị trạng thái kiểm tra tự động của CI (Checks):
     * 🟢 **Màu xanh (All checks have passed):** Mã nguồn an toàn, biên dịch tốt, vượt qua 72/72 ca kiểm thử và Docker hợp lệ. Nhóm trưởng hoặc thành viên có thể an tâm Review và Merge vào `main`.
     * 🔴 **Màu đỏ (Some checks were not successful):** Mã nguồn bị lỗi biên dịch hoặc có bài test bị rớt. **Tuyệt đối không được merge**. Thành viên phải xem log chi tiết để sửa lỗi tại máy cá nhân rồi push bổ sung.
4. **Sau khi Merge vào `main`:**
   * Pipeline sẽ tự động kích hoạt Giai đoạn 3 (CD) để đóng gói thành phẩm đưa vào mục **Artifacts**.

---

## 4. Hướng Dẫn Tải Thành Phẩm Đã Đóng Gói (Artifacts)

Giảng viên hoặc các thành viên muốn chạy thử nghiệm phiên bản mới nhất mà không cần cài đặt Visual Studio hay chạy lệnh build có thể tải bản đóng gói trực tiếp từ GitHub:

1. Truy cập vào kho lưu trữ GitHub của dự án.
2. Nhấn vào tab **Actions** trên thanh menu điều hướng.
3. Nhấn vào lần chạy mới nhất của quy trình **Blink Blink Jewelry - CI/CD Pipeline** (có dấu tích xanh 🟢).
4. Cuộn xuống phần **Artifacts** ở cuối trang:
   * Nhấn vào `blinkblink-admin-winforms-release` để tải tệp zip chứa ứng dụng Quản trị WinForms. Sau khi giải nén, nhấp đúp vào `BlinkBlink.Admin.exe` để chạy phần mềm quản trị và POS.
   * Nhấn vào `blinkblink-web-production-release` để tải gói ứng dụng Web khách hàng đã biên dịch sẵn.
   * Nhấn vào `ket-qua-kiem-thu-trx` nếu muốn xem báo cáo chi tiết 72 ca kiểm thử.

---

## 5. Huy Hiệu Trạng Thái Xây Dựng (Build Badge)

Để gắn huy hiệu hiển thị trạng thái CI/CD trực tiếp vào tệp `README.md` của dự án, bạn thêm đoạn mã Markdown sau vào đầu trang:

```markdown
![Blink Blink Jewelry CI/CD](https://github.com/<tai-khoan>/<kho-luu-tru>/actions/workflows/ci-cd.yml/badge.svg)
```
*(Thay thế `<tai-khoan>/<kho-luu-tru>` bằng đường dẫn repo của bạn, ví dụ: `sunsun0209s/Blink_Blink_Jewelry_Personal`).*

Huy hiệu sẽ tự động đổi màu xanh khi toàn bộ hệ thống hoạt động tốt, tạo sự chuyên nghiệp cho đồ án khi báo cáo nghiệm thu.

---

## 6. Xử Lý Sự Cố Thường Gặp (Troubleshooting)

| Sự cố | Nguyên nhân | Cách khắc phục |
| :--- | :--- | :--- |
| **Job 1 báo lỗi biên dịch WinForms** | Thiếu chỉ định môi trường Windows hoặc sử dụng runner Linux | Pipeline đã được cấu hình cố định `runs-on: windows-latest` để hỗ trợ trọn vẹn `net8.0-windows`. Không tự ý đổi sang `ubuntu-latest` cho Job 1. |
| **Job 1 báo lỗi bài kiểm thử rớt** | Một hàm nghiệp vụ vừa sửa làm thay đổi kết quả mong đợi của bài test | Chạy lệnh `dotnet test` tại máy cá nhân để xác định chính xác bài test bị lỗi, cập nhật lại logic hoặc dữ liệu mock trước khi push. |
| **Job 2 báo lỗi Dockerfile** | Thư mục build ngữ cảnh thiếu tệp hoặc sai đường dẫn `.csproj` | Kiểm tra lại cú pháp lệnh `COPY` trong `Dockerfile`. Chạy lệnh `docker build -f Dockerfile .` tại máy cá nhân để kiểm chứng. |
| **Job 3 không chạy sau khi push** | Commit được push lên nhánh con (`feature/*`), chưa phải nhánh `main` | Job 3 được thiết kế ràng buộc chỉ chạy trên nhánh `main` để tối ưu thời gian và tài nguyên máy ảo. Khi mở PR và gộp vào `main`, Job 3 sẽ tự động kích hoạt. |
