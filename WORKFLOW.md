# QUY TRÌNH LÀM VIỆC GIT & GITHUB DỰ ÁN BLINK BLINK JEWELRY
> **Tài liệu hướng dẫn chuẩn dành cho toàn bộ thành viên trong nhóm phát triển.**  
> Mục tiêu: Đảm bảo code chạy ổn định, không ghi đè code của nhau, không gây xung đột (conflict).

---

## 1. BA NGUYÊN TẮC BẤT DI BẤT DỊCH (QUY TẮC SỐNG CÒN)

1. **TUYỆT ĐỐI KHÔNG commit và push thẳng lên nhánh `main`:**
   - Nhánh `main` là code "sạch", code chuẩn đã chạy được và chỉ được cập nhật thông qua Pull Request (PR).
   - Mọi người chỉ được làm việc trên nhánh riêng của mình (`feature/...`).
2. **LUÔN LUÔN cập nhật code mới trước khi bắt đầu làm:**
   - Trước khi tạo nhánh mới hoặc trước khi bắt đầu một ngày làm việc, luôn kéo code mới nhất từ `main` về máy.
3. **TUYỆT ĐỐI KHÔNG dùng lệnh ép buộc nguy hiểm:**
   - Cấm dùng `git push --force` (hoặc `git push -f`). Lệnh này sẽ xóa sạch code của người khác trên GitHub.
   - Khi gặp lỗi hoặc bị conflict, dừng lại và báo ngay cho trưởng nhóm/thành viên chính để cùng xử lý.

---

## 2. QUY TRÌNH 6 BƯỚC HOÀN THÀNH MỘT TÍNH NĂNG (FEATURE)

Mỗi khi được phân công làm một nhiệm vụ (ví dụ: làm màn hình quản lý khách hàng, viết hàm tính điểm VIP, làm giao diện đăng nhập...), hãy thực hiện **đúng 6 bước** sau:

```
[Nhánh main] ──(Kéo code mới)──> [Tạo nhánh feature] ──(Code & Test)──> [Commit] ──(Push)──> [Tạo PR trên GitHub] ──(Review & Merge)──> [Nhánh main]
```

### Bước 1: Kéo code mới nhất từ nhánh `main` về máy
Mở Terminal / PowerShell tại thư mục dự án và chạy:
```bash
# 1. Chuyển về nhánh main
git checkout main

# 2. Tải toàn bộ code mới nhất của cả nhóm về
git pull origin main
```

---

### Bước 2: Tạo nhánh riêng cho tính năng của bạn
Đặt tên nhánh theo quy ước rõ ràng:
- `feature/ten-chuc-nang` (ví dụ: `feature/quan-ly-khach-hang`, `feature/giao-dien-pos`, `feature/form-login`)
- `fix/loi-can-sua` (nếu sửa lỗi: `fix/loi-tinh-gia-vang`)

Lệnh tạo và chuyển sang nhánh mới:
```bash
git checkout -b feature/ten-chuc-nang
```
*(Kiểm tra xem mình đang ở nhánh nào bằng lệnh: `git branch`)*

---

### Bước 3: Viết code và kiểm tra chạy thử
- Mở Visual Studio hoặc IDE để code tính năng được giao.
- **Rất quan trọng:** Trước khi lưu lại và gửi code, hãy bấm **Build Solution** (hoặc chạy lệnh `dotnet build BlinkBlinkJewelry.sln`).
- Đảm bảo chương trình **Build Succeeded (0 Errors)** thì mới chuyển sang Bước 4.

---

### Bước 4: Lưu lại thay đổi (Commit)
Nên commit thường xuyên sau khi hoàn thành xong một việc nhỏ, không nên gom cả tuần lại thành 1 commit to.

1. Kiểm tra danh sách các file mình đã sửa:
   ```bash
   git status
   ```
2. Thêm các file cần lưu vào danh sách chờ commit:
   ```bash
   # Thêm tất cả file đã thay đổi:
   git add .
   ```
3. Tạo commit với nội dung mô tả rõ ràng bằng tiếng Việt:
   ```bash
   git commit -m "Thêm giao diện quản lý khách hàng và danh sách thẻ VIP"
   ```
   *Quy ước ghi commit tốt:*
   - `Thêm giao diện FrmKhachHang trên WPF`
   - `Bổ sung Entity KhachHang và TheThanhVien trong tầng Domain`
   - `Sửa lỗi tính sai tiền công khi giảm giá VIP`
   - *(Tránh ghi nội dung vô nghĩa như: "fix", "update", "abc", "xong rồi")*

---

### Bước 5: Đẩy nhánh của bạn lên GitHub (Push)
Sau khi đã commit xong trên máy mình, đẩy nhánh đó lên GitHub:
```bash
git push origin feature/ten-chuc-nang
```
*(Lưu ý: Thay `feature/ten-chuc-nang` bằng đúng tên nhánh bạn đã tạo ở Bước 2).*

---

### Bước 6: Tạo Pull Request (PR) trên GitHub
1. Vào trình duyệt web, mở repository của nhóm:  
   👉 `https://github.com/sunsun0209s/Blink_Blink_Jewelry`
2. Bạn sẽ thấy một thanh thông báo màu vàng hiện lên kèm nút xanh: **"Compare & pull request"**. Bấm vào nút đó.
3. Điền mô tả ngắn gọn:
   - **Tiêu đề PR:** Tóm tắt tính năng vừa làm (VD: *Hoàn thành màn hình Quản lý Khách hàng*).
   - **Mô tả:** Nêu rõ các file/chức năng vừa bổ sung.
4. Chọn **Reviewers:** Chỉ định 1 trong 2 bạn phụ trách chính vào duyệt code.
5. Bấm nút xanh **"Create pull request"**.
6. **Không tự ý bấm Merge!** Hãy gửi link PR vào nhóm Zalo để 2 bạn chính vào kiểm tra code, chạy thử. Khi thấy ổn, các bạn chính sẽ bấm **Merge pull request** để gộp code vào `main`.

---

## 3. CÁCH XỬ LÝ CÁC TÌNH HUỐNG THƯỜNG GẶP

### Tình huống 1: Đang làm dở tính năng thì nhánh `main` có code mới của người khác
Muốn lấy code mới nhất trên `main` về đắp vào nhánh mình đang làm:
```bash
# 1. Lưu lại các file đang làm dở (nếu có) bằng commit tạm hoặc stash
git add .
git commit -m "Lưu tạm tiến độ"

# 2. Kéo code mới nhất từ main đắp vào nhánh hiện tại
git pull origin main
```
Nếu có xung đột (conflict), Visual Studio sẽ hiện giao diện trực quan cho bạn chọn giữ code cũ hay lấy code mới. Hãy hỏi bạn cùng nhóm nếu chưa chắc chắn!

---

### Tình huống 2: Lỡ sửa nhầm file lung tung và muốn hủy bỏ để quay về ban đầu
Nếu bạn sửa sai một file và muốn bỏ toàn bộ những gì vừa sửa để lấy lại code cũ:
```bash
# Hủy toàn bộ thay đổi chưa commit:
git restore .
```

---

### Tình huống 3: Làm xong 1 tính năng và muốn bắt đầu tính năng mới
Sau khi nhánh cũ đã được Merge trên GitHub:
```bash
# 1. Quay về main
git checkout main

# 2. Kéo toàn bộ code vừa merge về máy
git pull origin main

# 3. Tạo nhánh mới cho công việc tiếp theo
git checkout -b feature/chuc-nang-moi
```

---

## 4. BẢNG TÓM TẮT LỆNH DÙNG HẰNG NGÀY (CHEAT SHEET)

| Lệnh | Ý nghĩa |
| :--- | :--- |
| `git status` | Xem trạng thái file nào đang sửa, file nào chưa lưu |
| `git checkout main` | Quay về nhánh chính `main` |
| `git pull origin main` | Kéo code mới nhất từ GitHub về máy |
| `git checkout -b feature/xyz` | Tạo và nhảy sang nhánh mới `feature/xyz` |
| `git add .` | Chuẩn bị tất cả file đã sửa để chuẩn bị commit |
| `git commit -m "nội dung"` | Lưu lại kèm lời nhắn mô tả công việc |
| `git push origin feature/xyz` | Đẩy nhánh của mình lên GitHub |
| `dotnet build BlinkBlinkJewelry.sln` | Kiểm tra code có bị lỗi biên dịch không trước khi gửi |
