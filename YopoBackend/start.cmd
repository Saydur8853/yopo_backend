@echo off
echo Starting Yopo Backend API...
echo.
echo Make sure MySQL is running on localhost:3306
echo Database: yopo_backend
echo Username: root
echo Password: admin
echo.
echo The API will be available at:
echo - HTTP:  http://localhost:5000
echo - HTTPS: https://localhost:5001
echo - Swagger UI: https://localhost:5001/
echo.
pause
dotnet run
