@echo off
title Serveur CartoBureau
cd /d "%~dp0"
echo Recherche de l'adresse IP locale...
echo.
echo =======================================================
echo   Serveur CartoBureau Demarre !
echo =======================================================
echo   Adresse locale du PC : http://localhost:8080
echo.
echo   Acces depuis le reseau :
for /f "tokens=2 delims=:" %%i in ('ipconfig ^| findstr /i "IPv4"') do (
    set IP=%%i
    call :print_ip
)
echo.
echo   Gardez cette fenetre ouverte pour laisser le site actif.
echo =======================================================
echo.
python -u serveur.py
pause
goto :eof

:print_ip
set IP=%IP: =%
echo     - http://%IP%:8080
goto :eof
