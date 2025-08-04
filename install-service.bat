@echo off
set SCRIPT_PATH=%~dp0
set SERVICE_NAME=BiometricApiService
set DISPLAY_NAME=Biometric API (Control ID iDBio)
set EXE_PATH=%SCRIPT_PATH%BiometricApi.exe

ECHO.
ECHO ===================================================
ECHO  Instalando o servico da API Biometrica...
ECHO ===================================================
ECHO.

ECHO Criando o servico...
sc.exe create "%SERVICE_NAME%" binPath= "%EXE_PATH%" start= "auto" DisplayName= "%DISPLAY_NAME%"

ECHO.
ECHO Iniciando o servico...
sc.exe start "%SERVICE_NAME%"

ECHO.
ECHO Processo concluido. Esta janela fechara em 3 segundos...
timeout /t 3 /nobreak > nul