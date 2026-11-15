### Đây là nơi lưu lại những điều tối quan trọng để làm dự án không sai sót
## Hệ thống bao gồm kiến trúc clean architecture được viết bằng .Net, đối với trang dành cho admin sẽ được thiết kế bằng winform, còn trang dành cho người dùng mua sắm sẽ thiết kế bằng web (html, javascript, css, php nếu có). Web khách hàng sẽ giao tiếp với phần nghiệp vụ của clean architecture thông qua Restful API.
chương trình không cần quan tâm đến giá vàng hay loại vàng dùng cho sản phẩm này là gì, thông tin về cái đó chỉ cần lưu trong chuỗi string chất liệu là được, không cần ghi nhận lại lịch sử giá của sản phẩm đó.
3.1. Bắt buộc tham chiếu Taste Skill
BẮT BUỘC đọc và tuân thủ các quy tắc trong .agent/skills/design-taste-frontend/SKILL.md (hoặc SKILL.md).
### Quy trình làm việc
1. Kéo code mới nhất từ main
Thành viên vào nhánh main, kéo nhánh mới nhất của nhóm về máy:
git checkout main 
git pull origin main 
2. Tạo nhánh mới
git checkout -b feature/<tên tính năng>
làm việc tại đây, commit thoải mái không sợ ảnh hưởng đến người khác
3. Đẩy nhánh lên github
git push origin feature/<tên tính năng>
4. Mở Pull Request (PR) để ghép vào main
CI sẽ nằm tại đây
Lên giao diện GitHub, tạo một Pull Request: gộp từ feature/<tên tính năng> vào main
1 thành viên khác trong nhóm Review Code, comment nếu có lỗi
Quy ước đặt tên nhánh: 
feature/<tên tính năng>
bugfix/<lỗi cần sửa>
refactor/<tên module>

## Nguyên Tắc Cốt Lõi: Đọc Diagram Trước Tiên (Diagram-First Rule)
- Trước khi phân tích bất kỳ logic hoặc sinh mã nguồn nào, BẮT BUỘC phải đọc và đối chiếu các tài liệu/sơ đồ kiến trúc có sẵn trong project {
    Class Diagram.xml
    XML/
}
- Toàn bộ tên lớp, tên thuộc tính, kiểu dữ liệu, phương thức và các mối quan hệ (1-n, n-n, kế thừa, hợp thành) phải khớp 100% với bản vẽ trong Diagram.
- Tuyệt đối không tự ý bịa đặt, suy đoán hay đổi tên khác với Diagram khi chưa có yêu cầu từ người dùng.
- Nếu chưa nắm rõ luồng dữ liệu hoặc cấu trúc lớp từ diagram, phải chủ động yêu cầu người dùng chỉ định đường dẫn diagram hoặc làm rõ kiến trúc trước khi tiếp tục.
- Mọi giải pháp và code tạo ra phải tuân thủ nghiêm ngặt quan hệ giữa các thành phần đã định nghĩa trong sơ đồ.


## 2. Quy Trình Làm Việc: Lên Ý Tưởng Trước Khi Code
Khi nhận bất kỳ yêu cầu tính năng, tái cấu trúc hoặc fix bug nào, KHÔNG ĐƯỢC sinh code hoàn chỉnh ngay lập tức. Hãy thực hiện theo 2 bước:
### Bước 1: Đề xuất Ý tưởng & Giải pháp (Idea & Architectural Proposal)
Trình bày tóm tắt:
1. **Phân tích bối cảnh:** Dựa trên diagram/kiến trúc hiện tại, tính năng này tác động đến những module/lớp/bảng nào?
2. **Ý tưởng thực hiện:** Đưa ra 1–2 hướng tiếp cận khả thi kèm ưu/nhược điểm ngắn gọn.
3. **Kế hoạch file thay đổi:** Liệt kê các file dự kiến tạo mới hoặc chỉnh sửa.
4. **Dừng lại và chờ phản hồi:** Kết thúc phản hồi bằng câu hỏi xác nhận giải pháp từ người dùng.

### Bước 2: Thực thi (Implementation)
- CHỈ tiến hành viết mã nguồn sau khi người dùng đã duyệt và đồng ý với phương án ở Bước 1.

### 3. Quy Chuẩn Thiết Kế Giao Diện (Frontend & UI Taste Guidelines)
Khi nhận yêu cầu thiết kế, chỉnh sửa layout, giao diện

3.1. Bắt buộc tham chiếu Taste Skill
BẮT BUỘC đọc và tuân thủ các quy tắc trong .agent/skills/design-taste-frontend/SKILL.md (hoặc SKILL.md).

3.2. Các lệnh cấm tuyệt đối (Anti-Slop Bans)
CẤM dùng icon giả, placeholder méo mó, div giả product UI. Nếu cần hình ảnh trang sức minh họa, hãy dùng hình ảnh tư liệu thật (open-license) hoặc tài nguyên sẵn có của project.

Đối với WinForms POS/Admin: Giữ panel tinh tế, số liệu căn gióng chính xác (tabular figures), màu sắc chỉ tập trung hiển thị trạng thái dữ liệu (badge, status, tồn kho).
## 3. Quy Chuẩn Comment Code (Code Commenting Rules)

### Khi nào BẮT BUỘC comment:
- **Giải thích "Why" thay vì "What":** Không comment diễn giải lại code đang làm gì (tránh: `// Tăng i thêm 1` hay `// Lấy dữ liệu user`). Hãy giải thích lý do tại sao dùng thuật toán, công thức hoặc cách xử lý đó nếu logic phức tạp.
- **Header hàm/class chuẩn docstring:** Luôn có comment ngắn mô tả mục đích hàm, các tham số đầu vào (`@param`), giá trị trả về (`@return`) và ngoại lệ nếu có (`@throws`).
- **Liên kết sơ đồ/kiến trúc:** Với các hàm xử lý luồng chính, thêm comment ngắn chỉ rõ bước này tương ứng với bước nào trong Diagram/Tài liệu nghiệp vụ (ví dụ: `// Step 2 trong Flowchart: Xác thực OTP`).
- **Cảnh báo hoặc giả định (Edge Cases/Assumptions):** Ghi rõ điều kiện biên, ràng buộc dữ liệu hoặc cảnh báo nếu thay đổi logic này có thể làm ảnh hưởng module khác.

## Workflow Quản Lý Mục Tiêu & Tiến Độ (Task & Progress Tracking)

AI BẮT BUỘC tuân thủ chu trình quản lý task theo hai file: `TODO.md` (mục tiêu ngắn hạn cần làm) và `COMPLETED.md` (mục tiêu đã hoàn thành).

### 1. Khi Tiếp Nhận Yêu Cầu Mới
- **Đối chiếu `TODO.md`:** Luôn kiểm tra `TODO.md` để đảm bảo task hiện tại đồng nhất với mục tiêu ngắn hạn đã đề ra, tránh làm phân mảnh kiến trúc.
- **Tạo task mới (nếu chưa có):** Nếu người dùng yêu cầu một tính năng hoặc fix bug mới chưa có trong danh sách, hãy tự động bổ sung đầu việc vào mục `- [ ]` trong `TODO.md` trước khi bắt đầu thực thi.

### 2. Khi Hoàn Thành Một Task (Completion Lifecycle)
Sau khi code đã được kiểm tra, review và người dùng xác nhận hoàn thành:
1. **Cập nhật `TODO.md`:** Đổi trạng thái task thành `- [x]`.
2. **Chuyển sang `COMPLETED.md`:** 
   - Di chuyển task đã hoàn thành từ `TODO.md` sang file `COMPLETED.md` để giữ cho `TODO.md` luôn ngắn gọn, tránh lãng phí token context.
   - Định dạng ghi nhận vào `COMPLETED.md`:
     ```markdown
     - [x] [YYYY-MM-DD] [Tên Task]: Tóm tắt ngắn gọn các file/module đã thay đổi và chức năng hoàn tất.
     ```
3. **Báo cáo trạng thái kế tiếp:** Tóm tắt ngắn gọn task vừa hoàn thành và gợi ý task tiếp theo chưa tick trong `TODO.md` để người dùng quyết định bước kế tiếp.

## Quy Tắc Duy Trì Ngữ Cảnh Qua Các Phiên (Session Continuity)
- **Đầu mỗi phiên mới:** BẮT BUỘC kiểm tra file `SCRATCHPAD.md` và `TODO.md` để nắm trạng thái công việc dở dang gần nhất trước khi thực hiện task mới.
- **Cuối phiên / Trước khi reset chat:** Khi người dùng yêu cầu "lưu trạng thái" hoặc kết thúc task lớn, AI phải chủ động cập nhật `SCRATCHPAD.md` gồm 3 ý:
  1. Những gì đã hoàn thành (file nào đã sửa/tạo).
  2. Những gì đang làm dở và các quyết định kỹ thuật đã chốt.
  3. Hành động cụ thể tiếp theo cần làm cho phiên sau.
- **Dọn dẹp `SCRATCHPAD.md`:** Khi một task lớn đã hoàn thành trọn vẹn và đã chuyển sang `COMPLETED.md`, hãy làm sạch nội dung nháp trong `SCRATCHPAD.md` để sẵn sàng cho task tiếp theo.

### Những điều CẤM (Anti-patterns):
- KHÔNG viết comment thừa thãi, rác mã nguồn với các dòng code đã tự tường minh qua tên hàm/tên biến.
- Những code bị comment KHÔNG được tự tiện xoá vì nó dùng để test.
- Ngôn ngữ comment: Thống nhất dùng tiếng Việt chỉ dùng tiếng anh khi từ đó quá chuyên ngành.
- Tuyệt đối KHÔNG nâng Target Framework lên .NET 9 hay .NET 10 (thống nhất duy nhất .NET 8 LTS cho toàn solution).
- CẤM chú giải song ngữ thừa thãi: Không tự ý mở ngoặc chèn cụm từ tiếng Anh sau từ tiếng Việt (ví dụ: cấm viết "phân quyền (Role-based Access Control)" hay "tồn kho (Inventory)") trừ khi đó là những thuật ngữ dễ bị sai lệch nghĩa.

## 4. Quy Chuẩn Cơ Sở Dữ Liệu & Mã Hóa Tiếng Việt (Database Encoding & UTF-8 Rules)
- **Quy tắc nạp SQL Script bằng lệnh CLI:** Khi thực thi file `.sql` chứa văn bản tiếng Việt qua `sqlcmd`, BẮT BUỘC phải kèm cờ `-f 65001` (Code Page UTF-8), ví dụ: `sqlcmd -S . -f 65001 -i "database/BlinkBlinkJewelry.sql"`. Tuyệt đối CẤM chạy `sqlcmd -i` trần vì Windows sẽ đọc theo Codepage ANSI/OEM gây lỗi font rác (Mojibake: `Ã¢`, `á»`, `áº`).
- **Chuỗi ký tự trong SQL:** Mọi giá trị chuỗi tiếng Việt có dấu trong câu lệnh `INSERT`/`UPDATE` phải luôn có tiền tố `N'...'` (Unicode) và lưu vào cột kiểu `NVARCHAR`.
- **Định dạng file mã nguồn & SQL:** Toàn bộ file `.sql`, `.cs`, `.json`, `.js`, `.md` phải luôn được lưu ở định dạng chuẩn `UTF-8` (hoặc `UTF-8 with BOM`).