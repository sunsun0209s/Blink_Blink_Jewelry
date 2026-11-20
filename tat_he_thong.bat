@echo off
chcp 65001 > nul

echo ==============================================================================
echo       DANG TAT VA DON DEP TOAN BO CONTAINER BLINK BLINK JEWELRY
echo ==============================================================================
echo.

echo [1/2] Dang tat cac container Production (neu co)...
docker compose -f docker-compose.prod.yml down

echo [2/2] Dang tat cac container Development (neu co)...
docker compose -f docker-compose.dev.yml down

echo.
echo ==============================================================================
echo [OK] Toan bo he sinh thai container da duoc tat va don dep hoan toan!
echo ==============================================================================
timeout /t 3 /nobreak > nul
