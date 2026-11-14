@echo off
chcp 65001 > nul
echo ==============================================================================
echo       BLINK BLINK HAUTE JOAILLERIE - KHỞI ĐỘNG HỆ THỐNG TOÀN DIỆN
echo       1. Web Storefront Khách Hàng (http://localhost:5095 hoặc http://localhost:5000)
echo       2. Ứng Dụng WinForms Quản Trị Viên (Admin POS)
echo ==============================================================================
echo.

echo [1/2] Đang khởi động Web Server Khách Hàng (BlinkBlink.Web)...
start "BlinkBlink Web Server (Ports 5095 & 5000)" cmd /k "dotnet run --project src/BlinkBlink.Web --urls \"http://localhost:5095;http://localhost:5000\""

echo Đang chờ máy chủ web sẵn sàng trong 4 giây...
timeout /t 4 /nobreak > nul

echo Tự động mở trình duyệt Web Khách Hàng...
start http://localhost:5095/

echo.
echo [2/2] Đang khởi động Ứng dụng Desktop Quản trị (BlinkBlink.Admin)...
start "BlinkBlink WinForms Admin" cmd /c "dotnet run --project src/BlinkBlink.Admin"

echo.
echo Đã khởi động đồng thời cả 2 phân hệ thành công!
echo - Web Storefront: http://localhost:5095 (hoặc http://localhost:5000)
echo - Tài liệu Swagger API: http://localhost:5095/swagger
echo - Ứng dụng Quản trị WinForms: Đang khởi chạy
echo.
pause
