@echo off
chcp 65001 > nul
setlocal enabledelayedexpansion

echo ==============================================================================
echo       BLINK BLINK HAUTE JOAILLERIE - MOI TRUONG PHAT TRIEN [DEVELOPMENT]
echo       Co che tu dong: Nhan dien Docker hoac chay truc tiep tren may
echo ==============================================================================
echo.

echo [BUOC 1/3] Dang kiem tra trang thai Docker Desktop tren may...
docker info >nul 2>&1
if %ERRORLEVEL% equ 0 goto USE_DOCKER
goto USE_LOCAL

:USE_DOCKER
echo [OK] Phat hien Docker dang hoat dong.
echo      Khoi chay Docker Development: Hot-Reload, CSDL cong 1433...
docker compose -f docker-compose.dev.yml up -d --build
set "WEB_URL=http://localhost:5095"
goto READY_DEV

:USE_LOCAL
echo [INFO] Docker chua bat. Chay truc tiep tren may tinh...
echo        Khoi dong Web Server [ket noi CSDL cuc bo tren may]...
start "BlinkBlink Web Server (Development)" cmd /k "dotnet run --project src/BlinkBlink.Web --launch-profile http"
set "WEB_URL=http://localhost:5095"
goto READY_DEV

:READY_DEV
echo.
echo [BUOC 2/3] Dang cho he thong san sang (tham do ket noi tu dong)...
set /a RETRY_COUNT=0
:WAIT_WEB
timeout /t 1 /nobreak > nul
curl.exe -s -o nul !WEB_URL!/ >nul 2>&1
if %ERRORLEVEL% equ 0 goto WEB_READY
set /a RETRY_COUNT+=1
if !RETRY_COUNT! lss 30 (
    echo      Dang cho Web Server bien dich va mo cong... (!RETRY_COUNT!s)
    goto WAIT_WEB
)
:WEB_READY
echo [OK] May chu Web da san sang hoan toan.
echo Tu dong mo trinh duyet Web Storefront...
start !WEB_URL!/

echo.
echo [BUOC 3/3] Dang khoi dong Ung dung Quan tri WinForms [Admin POS]...
dotnet build src/BlinkBlink.Admin -v q --nologo
start "" "src\BlinkBlink.Admin\bin\Debug\net8.0-windows\BlinkBlink.Admin.exe"

echo.
echo ==============================================================================
echo       HE THONG PHAT TRIEN DA SAN SANG HOAT DONG
echo       - Web Khach Hang: !WEB_URL!/
echo       - Swagger API:    !WEB_URL!/swagger
echo       - WinForms Admin: Dang chay tren may
echo.
echo       >>> NHAN PHIM BAT KY DE DUNG HE THONG VA TAT CONTAINER DOCKER <<<
echo ==============================================================================
echo.
pause > nul

echo.
echo [DANG TAT] Dang tat cac dich vu va don dep container Docker...
docker compose -f docker-compose.dev.yml down >nul 2>&1
echo [OK] He thong da tat va don dep container hoan toan.
timeout /t 3 /nobreak > nul
pause
