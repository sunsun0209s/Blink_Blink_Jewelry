# HƯỚNG DẪN DÙNG GIT CHO NHÓM

## 1. Quy tắc bắt buộc
- Không code và không đẩy code thẳng lên nhánh `main`.
- Mỗi chức năng phải làm trên một nhánh riêng.
- Đầu ngày làm việc hoặc trước khi làm cái mới, luôn lấy code mới về máy.
- Cấm gõ lệnh `git push -f` (lệnh này sẽ ghi đè và làm mất code của người khác).

---

## 2. Các bước làm một chức năng

### Bước 1: Lấy code mới nhất về máy
- `git checkout main` : Chuyển về nhánh chính
- `git pull origin main` : Tải code mới nhất trên GitHub về máy

### Bước 2: Tạo nhánh riêng để làm
- `git checkout -b feature/ten-chuc-nang` : Tạo và chuyển sang nhánh mới để làm (ví dụ: `git checkout -b feature/khach-hang`, `git checkout -b feature/dang-nhap`)

### Bước 3: Viết code và kiểm tra
- Viết code xong, bấm chạy thử chương trình hoặc chạy lệnh `dotnet build` xem có bị báo lỗi đỏ không. Hết lỗi mới làm bước tiếp theo.

### Bước 4: Lưu code trên máy
- `git status` : Kiểm tra xem mình đã sửa những file nào
- `git add .` : Chọn tất cả file vừa sửa để chuẩn bị lưu
- `git commit -m "ghi ro minh vua lam gi"` : Lưu lại kèm ghi chú ngắn gọn (ví dụ: `git commit -m "them man hinh khach hang"`)

### Bước 5: Đẩy nhánh lên GitHub
- `git push origin feature/ten-chuc-nang` : Đẩy nhánh của mình lên web GitHub (thay đúng tên nhánh đã tạo ở Bước 2)

### Bước 6: Yêu cầu gộp code trên web GitHub
- Mở link GitHub của nhóm trên trình duyệt.
- Thấy thanh màu vàng hiện lên thì bấm nút **Compare & pull request**.
- Nhắn tin báo 2 bạn phụ trách chính vào kiểm tra code.
- Chờ 2 bạn chính duyệt và bấm gộp vào nhánh `main`, không tự bấm gộp.

---

## 3. Các trường hợp hay gặp

### Đang làm mà nhánh main có code mới, muốn lấy về nhánh của mình:
- `git add .` : Gom các file đang làm dở
- `git commit -m "luu tam"` : Lưu tạm lại vào máy
- `git pull origin main` : Kéo code mới từ main gộp vào nhánh đang làm

### Lỡ sửa sai muốn xóa hết để lấy lại code cũ ban đầu:
- `git restore .` : Hủy toàn bộ những thay đổi vừa sửa linh tinh

### Làm xong chức năng này, muốn bắt đầu chức năng khác:
- `git checkout main` : Quay về nhánh chính
- `git pull origin main` : Kéo code vừa được gộp về máy
- `git checkout -b feature/ten-chuc-nang-moi` : Tạo nhánh mới tiếp theo
