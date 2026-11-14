@echo off
chcp 65001 > nul
echo ==============================================================================
echo       BLINK BLINK HAUTE JOAILLERIE - WEB STOREFRONT KHÁCH HÀNG
echo       Kênh Mua Sắm Trang Sức Trực Tuyến & RESTful API
echo ==============================================================================
echo.

echo Đang khởi động Web Server trên cả 2 cổng 5095 và 5000...
start "BlinkBlink Web Server (Ports 5095 & 5000)" cmd /k "dotnet run --project src/BlinkBlink.Web --urls \"http://localhost:5095;http://localhost:5000\""

echo Đang chờ máy chủ web sẵn sàng trong 4 giây...
timeout /t 4 /nobreak > nul

echo Tự động mở trình duyệt Web Khách Hàng...
start http://localhost:5095/

echo.
echo Máy chủ Web đã hoạt động:
echo - Cổng mua sắm chính: http://localhost:5095
echo - Cổng phụ / Docker dev: http://localhost:5000
echo - Tài liệu Swagger API: http://localhost:5095/swagger
echo.
pause
