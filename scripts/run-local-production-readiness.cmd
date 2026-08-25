@echo off
echo Executando Production Readiness local...
echo Se o PowerShell bloquear execucao, rode como Administrador ou use ExecutionPolicy Bypass.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-local-production-readiness.ps1" %*
exit /b %ERRORLEVEL%
