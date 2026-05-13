@echo off
chcp 65001 >nul
echo ========================================================
echo Запуск системы Кадастрового Учета и Оценки
echo ========================================================
echo.
echo Проверка наличия Docker...
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ОШИБКА] Docker не установлен или не запущен!
    echo Пожалуйста, установите Docker Desktop и запустите его.
    pause
    exit /b
)

echo.
echo Остановка старых контейнеров (если есть)...
docker-compose down

echo.
echo Сборка и запуск (порт 9000)...
docker-compose up -d --build

echo.
echo ========================================================
echo Система запущена на порту 9000!
echo Открываю браузер: http://localhost:9000
echo ========================================================

timeout /t 5 /nobreak >nul
start http://localhost:9000

pause