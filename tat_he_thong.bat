@echo off
chcp 65001 > nul
cd /d "%~dp0"

echo ==============================================================================
echo       DANG TAT VA DON DEP TOAN BO HE THONG BLINK BLINK JEWELRY
echo ==============================================================================
echo.

echo [1/3] Dang dong Ung dung Quan tri WinForms Admin (neu co)...
taskkill /f /im BlinkBlink.Admin.exe >nul 2>&1
taskkill /f /im ngrok.exe >nul 2>&1
echo [OK] Da dong cac tien trinh ung dung desktop.

echo.
echo [2/3] Dang kiem tra va tat cac container Docker...
docker info >nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo      Dang dung container Production...
    docker compose -f docker-compose.prod.yml down >nul 2>&1
    echo      Dang dung container Development...
    docker compose -f docker-compose.dev.yml down >nul 2>&1
    echo [OK] Toan bo container Docker da duoc don dep hoan toan.
) else (
    echo [INFO] Docker Desktop khong hoat dong hoac da tat truoc do.
)

taskkill /fi "WINDOWTITLE eq BlinkBlink Web Server*" /f >nul 2>&1

echo.
echo ==============================================================================
echo [THANH CONG] He thong da duoc tat va don dep sach se hoan toan!
echo.
echo Nhan phim bat ky de dong cua so nay...
echo ==============================================================================
pause > nul
