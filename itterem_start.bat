@echo off

:: =============================================================================
:: 1. TAKARÍTÁS (Folyamatok leállítása, hogy a portok felszabaduljanak)
:: =============================================================================
echo Szolgaltatasok leallitasa...

:: Csak a hatterfolyamatokat lojjuk le, a Terminalt nem, hogy a script vegigfusson
taskkill /F /IM "dotnet.exe" /T 2>nul
taskkill /F /IM "nginx.exe" /T 2>nul
taskkill /F /IM "ngrok.exe" /T 2>nul
taskkill /F /IM "mysqld.exe" /T 2>nul

timeout /t 2 /nobreak >nul

:: =============================================================================
:: 2. MYSQL SZERVER INDÍTÁSA (Útvonal hiba javítva)
:: =============================================================================
echo MySQL szerver inditasa...

:: Belepunk a XAMPP mappaba, igy a mysql_start mar latni fogja az utvonalakat
cd /d "C:\xampp"
:: Azonnal inditjuk a hatterben, a hibauzeneteket elnyomjuk, hogy ne nyisson extra ablakot
start /B "" "C:\xampp\mysql_start.bat" >nul 2>&1

:: Varunk 4 masodpercet, hogy a motor beinduljon
timeout /t 4 /nobreak >nul

:: =============================================================================
:: 3. ÚJ WINDOWS TERMINAL INDÍTÁSA (A 4 fontos füllel)
:: =============================================================================
echo Az uj munkaterulet megnyitasa...

:: A 'start' parancs futtatja a Terminalt kulon folyamatkent
start wt ^
  -d "C:\Users\kulcs\Documents\GitHub\itterem-back-end-\vizsgaremek" --title "1. BACK-END" cmd /k "dotnet run --urls="http://127.0.0.1:7200"" ; ^
  nt -d "C:\xampp\mysql\bin" --title "2. MYSQL KLIENS" cmd /k "mysql.exe -u root" ; ^
  nt -d "C:\Users\kulcs\Documents\nginx" --title "3. NGINX" cmd /k "nginx" ; ^
  nt -d "C:\Users\kulcs\Documents\ngrok-v3-stable-windows-amd64" --title "4. NGROK" cmd /k "ngrok http 8080"

:: =============================================================================
:: 4. AZ INDÍTÓ (FEKETE) ABLAK BEZÁRÁSA
:: =============================================================================
:: Ez a parancs bezárja ezt az ablakot, de a 'start' miatt az uj Terminal marad!
exit