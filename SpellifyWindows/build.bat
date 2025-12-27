@echo off
echo Building Spellify...
dotnet publish -c Release -o dist

echo.
echo Creating installer...
if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" (
    "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" installer.iss
    echo.
    echo Done! Installer is in the 'installer' folder.
) else (
    echo Inno Setup not found. Install from: https://jrsoftware.org/isdl.php
    echo.
    echo Portable exe is in the 'dist' folder.
)
pause
