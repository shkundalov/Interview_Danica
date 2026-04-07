@echo off
del /f /q "%~dp0InterviewDanica.API\interview_danica.db"
del /f /q "%~dp0InterviewDanica.API\interview_danica.db-shm"
del /f /q "%~dp0InterviewDanica.API\interview_danica.db-wal"
dotnet build
start "Server" dotnet run --project InterviewDanica.Api --no-build
timeout /t 2 /nobreak
dotnet test InterviewDanica.Api.Tests --no-build --logger "console;verbosity=detailed"
pause
taskkill /F /IM dotnet.exe 2>&1 | findstr "SUCCESS"