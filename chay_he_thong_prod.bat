@echo off
chcp 65001 > nul
setlocal enabledelayedexpansion

echo ==============================================================================
echo       BLINK BLINK HAUTE JOAILLERIE - MOI TRUONG VAN HANH [PRODUCTION]
echo       Kem duong ham ngrok cong khai va ung dung Quan tri WinForms
echo ==============================================================================
echo.

echo [BUOC 1/4] Dang kiem tra trang thai Docker Desktop tren may...
docker info >nul 2>&1
if %ERRORLEVEL% equ 0 goto USE_DOCKER_PROD
goto USE_LOCAL_PROD

:USE_DOCKER_PROD
echo [OK] Phat hien Docker dang hoat dong.
echo      Dang tai ban dong goi Web moi nhat tu GitHub Packages [ghcr.io]...
docker compose -f docker-compose.prod.yml pull web
echo      Khoi chay he sinh thai Docker Production [CSDL cong 1433]...
docker compose -f docker-compose.prod.yml up -d
set "WEB_PORT=8080"
set "WEB_URL=http://localhost:8080"
goto OPEN_NGROK

:USE_LOCAL_PROD
echo [INFO] Docker chua bat. Chay truc tiep tren may tinh...
echo        Khoi dong Web Server che do Release [ket noi CSDL cuc bo tren may]...
start "BlinkBlink Web Server (Production Mode)" cmd /k "dotnet run --project src/BlinkBlink.Web --configuration Release --launch-profile http"
set "WEB_PORT=5095"
set "WEB_URL=http://localhost:5095"
goto OPEN_NGROK

:OPEN_NGROK
echo.
echo [BUOC 2/4] Dang khoi dong duong ham ngrok cong khai...
where ngrok >nul 2>&1
if %ERRORLEVEL% equ 0 (
    start "BlinkBlink Ngrok Tunnel" cmd /k "echo Dang ket noi ham ngrok vao cong !WEB_PORT!... & ngrok http !WEB_PORT!"
    echo [OK] Da mo cua so ngrok. Vui long sao chep duong link Forwarding [https://...ngrok-free.app] de chia se.
) else (
    echo [!] Canh bao: Máy chưa cài đặt công cụ ngrok hoặc chưa cấu hình PATH.
    echo     Ban có thể cài đặt bằng lệnh: winget install ngrok
)

echo.
echo [BUOC 3/4] Dang cho he thong san sang trong 4 giay...
timeout /t 4 /nobreak > nul

echo Tu dong mo trinh duyet Web Khach Hang...
start !WEB_URL!/

echo.
echo [BUOC 4/4] Dang khoi dong Ung dung Quan tri WinForms [Admin POS]...
dotnet build src/BlinkBlink.Admin -c Release -v q --nologo
start "" "src\BlinkBlink.Admin\bin\Release\net8.0-windows\BlinkBlink.Admin.exe"

echo.
echo ==============================================================================
echo       HE THONG PRODUCTION DA HOAT DONG
echo       - Web Khach Hang [Noi bo]: !WEB_URL!/
echo       - Duong ham ngrok: Dang hien thi o cua so ngrok rieng
echo       - WinForms Admin: Dang chay tren may tinh
echo ==============================================================================
echo.
pause
