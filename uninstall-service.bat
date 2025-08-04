@echo off
set SERVICE_NAME=BiometricApiService

ECHO.
ECHO =====================================================
ECHO  Desinstalando o servico da API Biometrica...
ECHO =====================================================
ECHO.

ECHO Parando o servico...
sc.exe stop "%SERVICE_NAME%" > NUL 2>&1

ECHO Aguardando o encerramento completo do servico...
:WAITLOOP
timeout /t 1 > nul
sc query "%SERVICE_NAME%" | findstr /C:"STOPPED" > nul
if errorlevel 1 goto WAITLOOP

ECHO Removendo o servico...
sc.exe delete "%SERVICE_NAME%"

ECHO.
ECHO Processo concluido. Esta janela fechara em 3 segundos...
timeout /t 3 /nobreak > nul