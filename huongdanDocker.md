# HƯỚNG DẪN SỬ DỤNG VÀ SO SÁNH 2 CẤU HÌNH DOCKER (DEVELOPMENT & PRODUCTION)
## DỰ ÁN TRANG SỨC BLINK BLINK JEWELRY (.NET 8 LTS - CLEAN ARCHITECTURE)

---

## 1. TỔNG QUAN KIẾN TRÚC DOCKER TRONG ĐỒ ÁN

Theo yêu cầu chuẩn hóa quy trình phát triển và triển khai phần mềm, hệ thống được thiết kế với **2 cấu hình Docker độc lập**:

1. **Docker dành cho Lập Trình Viên (Development):** Tối ưu cho quy trình lập trình, kiểm thử liên tục, tự động biên dịch lại khi sửa mã nguồn (Hot Reload), mở cổng cơ sở dữ liệu để các công cụ ngoài (SSMS, VS Code) và ứng dụng WinForms Admin chạy trên máy tính kết nối trực tiếp.
2. **Docker dành cho Người Dùng Triển Khai (Production):** Tối ưu cho môi trường vận hành thực tế của khách hàng, đóng gói đa tầng siêu nhẹ, bảo mật tuyệt đối (đóng kín cổng CSDL trong mạng nội bộ), kích hoạt cơ chế xử lý lỗi và ghi nhật ký thích ứng để không lộ thông tin nhạy cảm.

---

## 2. BẢNG SO SÁNH CHI TIẾT SỰ KHÁC BIỆT GIỮA 2 DOCKER

Dưới đây là bảng phân tích toàn diện các điểm khác biệt giữa 2 cấu hình Docker để báo cáo và giải trình với giảng viên:

| Tiêu Chí So Sánh | Docker Lập Trình Viên (Development) | Docker Người Dùng Triển Khai (Production) | Ý Nghĩa Thực Tế Trong Đồ Án |
| :--- | :--- | :--- | :--- |
| **Mục đích phục vụ** | Lập trình viên phát triển, sửa lỗi và kiểm thử tính năng | Người dùng cuối mua sắm và vận hành thực tế hệ thống | Tách biệt hoàn toàn giữa giai đoạn chế tạo và giai đoạn vận hành thương mại |
| **Tệp cấu hình** | `Dockerfile.dev`<br>`docker-compose.dev.yml` | `Dockerfile`<br>`docker-compose.prod.yml` | Rõ ràng, không bị chồng chéo cờ lệnh khi thao tác |
| **Kỹ thuật đóng gói** | Đóng gói đơn tầng trên nền tảng .NET 8 SDK đầy đủ | Đóng gói đa tầng (Multi-stage build): Build bằng SDK, chạy trên Runtime | Giảm tối đa kích thước container khi triển khai thực tế |
| **Hình ảnh nền (Base Image)** | `mcr.microsoft.com/dotnet/sdk:8.0` (~800 MB) | `mcr.microsoft.com/dotnet/aspnet:8.0` (~200 MB) | Tiết kiệm băng thông máy chủ và tài nguyên lưu trữ |
| **Biến môi trường** | `ASPNETCORE_ENVIRONMENT=Development` | `ASPNETCORE_ENVIRONMENT=Production` | Kích hoạt đúng nhánh hành vi thích ứng trong toàn bộ mã nguồn |
| **Cơ chế nạp mã nguồn** | Dùng lệnh `dotnet watch` kết hợp gắn kết thư mục (`volume mount` `./src:/app/src`) | Biên dịch sẵn gói phát hành tối ưu (`dotnet publish -c Release`) | Lập trình viên sửa code thấy ngay; bản người dùng cố định, ổn định cao |
| **Xử lý lỗi hệ thống** | Hiển thị chi tiết lỗi kỹ thuật, thông điệp Exception và toàn bộ vết ngăn xếp (Stack Trace) | Cơ chế thích ứng: Ẩn toàn bộ chi tiết hệ thống, chỉ trả về mã lỗi chuẩn và mã đối soát | Chống lộ cấu trúc mã nguồn, cấu trúc thư mục và bảo vệ an toàn thông tin |
| **Mức độ ghi nhật ký (Log)** | Ghi nhận chi tiết từ mức `Debug` trở lên | Chỉ ghi nhận từ mức `Information`, `Warning`, `Error` trở lên | Tránh lãng phí dung lượng đĩa và hạn chế nghẽn I/O khi có nhiều người truy cập |
| **Giao diện Swagger API** | Mở công khai (`/swagger`) để kiểm thử các điểm cuối RESTful API | Tắt giao diện tài liệu công khai để bảo mật các ngõ giao tiếp | Ngăn chặn việc dò quét lộ điểm cuối API của hệ thống từ người ngoài |
| **Cổng Web phục vụ** | Cổng `5000` và `5095` (`http://localhost:5000`) | Cổng chuẩn `8080` (`http://localhost:8080`) | Khách hàng truy cập dễ dàng qua cổng web mặc định |
| **Cổng CSDL SQL Server** | **Mở cổng 1433 ra máy thật (`1433:1433`)** | **Đóng kín cổng 1433 trong mạng nội bộ Docker** | Cho phép lập trình viên dùng SSMS và WinForms Admin kết nối; bảo vệ tuyệt đối dữ liệu Production |
| **Chính sách tự phục hồi** | Không bật tự khởi động lại (để lập trình viên dễ quan sát lỗi dừng) | Bật chính sách tự phục hồi (`restart: unless-stopped`) | Hệ thống người dùng tự bật lại ngay nếu máy chủ khởi động lại hoặc container gặp sự cố |
| **Quản lý dữ liệu CSDL** | Dùng volume phát triển `mssql_dev_data` | Dùng volume vận hành bền vững `mssql_prod_data` | Tránh việc dữ liệu test trong lúc code làm sai lệch dữ liệu kinh doanh thật |

---

## 3. HƯỚNG DẪN SỬ DỤNG DOCKER 1: MÔI TRƯỜNG LẬP TRÌNH VIÊN (DEVELOPMENT)

### 3.1. Mục Đích Sử Dụng
Dùng khi đang viết mã nguồn, sửa lỗi giao diện, thêm hàm API hoặc khi cần chạy ứng dụng Desktop WinForms Admin trên máy kết nối vào CSDL SQL Server đang chạy trong Docker.

### 3.2. Lệnh Khởi Chạy
Mở PowerShell tại thư mục gốc của đồ án và thực thi lệnh:
```bash
docker compose -f docker-compose.dev.yml up --build -d
```

### 3.3. Các Địa Chỉ Truy Cập Môi Trường Phát Triển
- **Giao diện Web khách hàng:** [http://localhost:5000](http://localhost:5000) hoặc [http://localhost:5095](http://localhost:5095)
- **Giao diện tài liệu Swagger API:** [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **Máy chủ CSDL SQL Server:** `localhost,1433`
  - Tên đăng nhập: `sa`
  - Mật khẩu: `BlinkBlink@Dev2026`
  - Cơ sở dữ liệu: `BlinkBlinkJewelryDb` (Hệ thống tự động chạy kịch bản `BlinkBlinkJewelry.sql` khởi tạo bảng và dữ liệu mẫu khi Web khởi động).

### 3.4. Cơ Chế Tự Động Nạp Lại Mã Nguồn (Hot Reload)
- Bạn chỉ cần mở tệp mã nguồn trong thư mục `src/` (ví dụ file Controller, Razor View, CSS, JS hoặc UseCase) và nhấn **Save** (`Ctrl + S`).
- Nhờ cơ chế `dotnet watch` và gắn kết volume, container sẽ tự động phát hiện thay đổi và biên dịch lại ngay lập tức mà **không cần chạy lại lệnh build container**.

### 3.5. Hướng Dẫn Kết Nối Ứng Dụng WinForms Admin Trên Máy Vào CSDL Docker
Khi máy tính của bạn không cài SQL Server cục bộ, bạn có thể chạy WinForms Admin kết nối thẳng vào CSDL trong Docker:
1. Đảm bảo Docker Development đang chạy (cổng `1433` đã mở).
2. Mở giải pháp `BlinkBlinkJewelry.slnx` trên Visual Studio và bấm chạy dự án `BlinkBlink.Admin` (hoặc chạy lệnh `dotnet run --project src/BlinkBlink.Admin`).
3. Phần mềm WinForms sẽ tự động dò tìm máy chủ `localhost` và kết nối trơn tru vào cơ sở dữ liệu `BlinkBlinkJewelryDb` bên trong Docker.

### 3.6. Xem Nhật Ký Chẩn Đoán (Log)
```bash
# Xem log của dịch vụ Web đang chạy
docker logs -f blinkblink_web_dev

# Xem log của máy chủ SQL Server
docker logs -f blinkblink_sql_dev
```

### 3.7. Dừng Môi Trường Phát Triển
```bash
# Dừng các container nhưng vẫn giữ lại dữ liệu CSDL
docker compose -f docker-compose.dev.yml down

# Dừng và xóa sạch dữ liệu để nạp lại CSDL mới tinh từ đầu
docker compose -f docker-compose.dev.yml down -v
```

---

## 4. HƯỚNG DẪN SỬ DỤNG DOCKER 2: MÔI TRƯỜNG NGƯỜI DÙNG TRIỂN KHAI (PRODUCTION)

### 4.1. Mục Đích Sử Dụng
Dùng khi nghiệm thu với giảng viên, thuyết trình đồ án hoặc triển khai lên máy chủ thật cho người dùng truy cập mua sắm trang sức.

### 4.2. Lệnh Khởi Chạy
Mở PowerShell tại thư mục gốc của đồ án và thực thi lệnh:
```bash
docker compose -f docker-compose.prod.yml up --build -d
```

### 4.3. Các Địa Chỉ Truy Cập Môi Trường Vận Hành
- **Giao diện Web khách hàng mua sắm:** [http://localhost:8080](http://localhost:8080)
- **Tài liệu Swagger:** Đã được tự động tắt để bảo đảm an toàn hệ thống.
- **Cơ sở dữ liệu:** Nằm khép kín hoàn toàn trong mạng nội bộ `blinkblink_prod_net`, người ngoài không thể kết nối trực tiếp vào cổng 1433 để dò quét mật khẩu quản trị.

### 4.4. Cơ Chế Thích Ứng Log & Error Ở Môi Trường Vận Hành
- **Bảo mật phản hồi lỗi:** Khi có lỗi phát sinh (ví dụ: giỏ hàng trống, hết hàng, mã sản phẩm không tồn tại), hệ thống chỉ trả về vỏ bọc JSON chuẩn hóa:
  ```json
  {
    "success": false,
    "errorCode": "OUT_OF_STOCK",
    "message": "Số lượng sản phẩm trong kho không đủ đáp ứng",
    "correlationId": "8A2B9C1D"
  }
  ```
  Toàn bộ thông tin đường dẫn file, dòng code lỗi và tên bảng CSDL đều được ẩn đi.
- **Vị trí lưu nhật ký:** Tệp nhật ký JSON xoay vòng theo ngày được lưu bền vững trong volume `web_prod_logs` tại đường dẫn `/app/logs/web-yyyyMMdd.log` bên trong container.

### 4.5. Xem Nhật Ký Vận Hành
```bash
# Xem dòng nhật ký Web trong container Production
docker logs -f blinkblink_web_prod

# Xem nội dung tệp nhật ký lưu trên đĩa container
docker exec -it blinkblink_web_prod sh -c "cat /app/logs/web-*.log"
```

### 4.6. Dừng Môi Trường Vận Hành
```bash
# Dừng dịch vụ an toàn (dữ liệu CSDL và nhật ký vẫn được lưu giữ)
docker compose -f docker-compose.prod.yml down
```

---

## 5. CÁC LỆNH TIỆN ÍCH VÀ XỬ LÝ SỰ CỐ THƯỜNG GẶP (TROUBLESHOOTING)

### 5.1. Kiểm Tra Trạng Thái Sức Khỏe Của Các Container
Để kiểm tra xem container Web và CSDL đã sẵn sàng hay chưa, gõ lệnh:
```bash
docker ps
```
Quan sát cột `STATUS`, nếu thấy hiển thị `Up ... (healthy)` nghĩa là máy chủ SQL Server đã hoàn tất kiểm tra sức khỏe và Web đã kết nối thành công.

### 5.2. Khắc Phục Lỗi Xung Đột Cổng (Port Conflict)
- **Lỗi cổng 1433 bị chiếm dụng:** Nếu máy tính của bạn đã cài sẵn dịch vụ SQL Server chạy nền ở cổng 1433, khi chạy Docker Development có thể báo lỗi cổng đã được sử dụng.
  - *Cách xử lý nhanh:* Mở `Services.msc` trên Windows, tìm dịch vụ `SQL Server (MSSQLSERVER)` và bấm **Stop** tạm thời trước khi khởi chạy Docker Development.
- **Lỗi cổng 5000 hoặc 8080 bị chiếm dụng:** Đổi cổng bên trái trong tệp docker-compose (ví dụ đổi `"5001:5000"` hoặc `"8081:8080"`).

### 5.3. Khởi Tạo Lại Toàn Bộ Dữ Liệu CSDL Về Mặc Định
Nếu trong quá trình thao tác bạn nhập dữ liệu thử nghiệm bị sai lệch và muốn đưa CSDL về trạng thái ban đầu của file `BlinkBlinkJewelry.sql`:
```bash
# Đối với môi trường Lập trình viên:
docker compose -f docker-compose.dev.yml down -v
docker compose -f docker-compose.dev.yml up --build -d

# Đối với môi trường Người dùng vận hành:
docker compose -f docker-compose.prod.yml down -v
docker compose -f docker-compose.prod.yml up --build -d
```
Cờ `-v` sẽ xóa sạch volume cũ để hệ thống tự động chạy lại kịch bản SQL nạp dữ liệu chuẩn mới.

---

## 6. DANH MỤC CÁC TỆP CẤU HÌNH DOCKER TRONG ĐỒ ÁN

- [.dockerignore](file:///.dockerignore): Tệp loại trừ các thư mục rác và tệp tạm biên dịch.
- [Dockerfile.dev](file:///Dockerfile.dev): Kịch bản đóng gói container cho môi trường phát triển (.NET 8 SDK + Hot Reload).
- [docker-compose.dev.yml](file:///docker-compose.dev.yml): Cụm dịch vụ phát triển (Web Dev + SQL Server mở cổng 1433).
- [Dockerfile](file:///Dockerfile): Kịch bản đóng gói đa tầng tối ưu hóa cho môi trường triển khai (.NET 8 Runtime).
- [docker-compose.prod.yml](file:///docker-compose.prod.yml): Cụm dịch vụ triển khai vận hành (Web Prod + SQL Server bảo mật khép kín).
- [huongdanDocker.md](file:///huongdanDocker.md): Bản đặc tả và tài liệu hướng dẫn vận hành chi tiết.
