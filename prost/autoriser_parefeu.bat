@echo off
title Configuration Pare-feu CartoBureau
:: Vérifier les privilèges d'administrateur
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"

if '%errorlevel%' NEQ '0' (
    echo Demande d'autorisation administrateur...
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    echo UAC.ShellExecute "%~s0", "", "", "runas", 1 >> "%temp%\getadmin.vbs"
    "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    if exist "%temp%\getadmin.vbs" ( del "%temp%\getadmin.vbs" )
    pushd "%CD%"
    CD /D "%~dp0"
    
    echo =======================================================
    echo   Configuration du Pare-feu Windows
    echo =======================================================
    echo.
    powershell -Command "New-NetFirewallRule -DisplayName 'CartoBureau Server' -Direction Inbound -LocalPort 8080 -Protocol TCP -Action Allow -Force"
    echo.
    echo Règle ajoutee ! Les autres appareils peuvent se connecter.
    echo =======================================================
    echo.
    pause
