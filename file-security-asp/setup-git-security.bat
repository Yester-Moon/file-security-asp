@echo off
REM Git Security Setup Script for Windows
REM File Security System

echo.
echo ======================================================================
echo                   Git Security Setup
echo                 File Security System
echo ======================================================================
echo.

REM Check if git repository
if not exist ".git" (
    echo ERROR: Not a git repository!
    exit /b 1
)

echo [OK] Git repository detected
echo.

REM Install pre-commit hook
echo ======================================================================
echo Installing Pre-commit Hook
echo ======================================================================
echo.

if exist ".git\hooks\pre-commit" (
    echo WARNING: Pre-commit hook already exists
    set /p "overwrite=Overwrite? (y/N): "
    if /I not "%overwrite%"=="y" (
        echo Skipping hook installation
        goto :check_history
    )
)

if exist "pre-commit.bat" (
    copy /Y pre-commit.bat .git\hooks\pre-commit >nul
    echo [OK] Pre-commit hook installed
) else (
    echo ERROR: pre-commit.bat not found!
    exit /b 1
)

:check_history
echo.
echo ======================================================================
echo Checking Git History
echo ======================================================================
echo.

git log --all --full-history --source --name-only -- "*appsettings.json" 2>nul | findstr /I "appsettings.json" >nul
if %errorlevel% equ 0 (
    echo WARNING: Found appsettings.json in git history!
    echo You MUST clean the git history!
    echo See GIT_SECURITY.md for instructions
) else (
    echo [OK] No sensitive files found in git history
)

echo.
echo ======================================================================
echo Removing Tracked Sensitive Files
echo ======================================================================
echo.

git ls-files | findstr /I "appsettings.json" >nul 2>&1
if %errorlevel% equ 0 (
    echo Removing appsettings.json from git tracking...
    git rm --cached FileService\FileService.WebAPI\appsettings.json 2>nul
    echo Files removed from tracking. Don't forget to commit!
) else (
    echo [OK] No tracked sensitive files found
)

echo.
echo ======================================================================
echo Creating Configuration Files
echo ======================================================================
echo.

set TEMPLATE=FileService\FileService.WebAPI\appsettings.Development.json.example
set CONFIG=FileService\FileService.WebAPI\appsettings.json

if not exist "%TEMPLATE%" (
    echo ERROR: Template file not found: %TEMPLATE%
    goto :generate_keys
)

if exist "%CONFIG%" (
    echo WARNING: Configuration file already exists
    set /p "overwrite=Overwrite? (y/N): "
    if /I not "%overwrite%"=="y" (
        echo Skipping configuration creation
        goto :generate_keys
    )
)

copy /Y "%TEMPLATE%" "%CONFIG%" >nul
echo [OK] Configuration file created from template
echo WARNING: Don't forget to update with your actual values!

:generate_keys
echo.
echo ======================================================================
echo Generate Secure Keys?
echo ======================================================================
echo.

set /p "genkeys=Generate secure keys using PowerShell? (y/N): "
if /I not "%genkeys%"=="y" goto :test_hook

echo.
echo Generating keys...
echo.

powershell -Command ^
"$jwtKeyBytes = New-Object byte[] 64; ^
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($jwtKeyBytes); ^
$jwtKey = [Convert]::ToBase64String($jwtKeyBytes); ^
Write-Host 'JWT Key:' -ForegroundColor Green; ^
Write-Host $jwtKey; ^
Write-Host ''; ^
$aesKeyBytes = New-Object byte[] 32; ^
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($aesKeyBytes); ^
$aesKey = [Convert]::ToBase64String($aesKeyBytes); ^
Write-Host 'AES Key:' -ForegroundColor Green; ^
Write-Host $aesKey; ^
Write-Host ''; ^
$aesIvBytes = New-Object byte[] 16; ^
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($aesIvBytes); ^
$aesIv = [Convert]::ToBase64String($aesIvBytes); ^
Write-Host 'AES IV:' -ForegroundColor Green; ^
Write-Host $aesIv"

echo.
echo WARNING: Save these keys securely!
echo.
pause

:test_hook
echo.
echo ======================================================================
echo Testing Pre-commit Hook
echo ======================================================================
echo.

echo {"test": "data"} > .test_file.json
git add .test_file.json 2>nul
git commit -m "Test" --dry-run 2>nul
if %errorlevel% neq 0 (
    echo [OK] Pre-commit hook is working!
) else (
    echo WARNING: Pre-commit hook might not be working correctly
)
git reset HEAD .test_file.json 2>nul
del .test_file.json 2>nul

echo.
echo ======================================================================
echo Setup Complete!
echo ======================================================================
echo.

echo Next steps:
echo   1. Update appsettings.json with your values
echo   2. If sensitive files were found, clean git history (see GIT_SECURITY.md)
echo   3. If files were removed, commit the changes:
echo      git commit -m "Remove sensitive files from version control"
echo   4. Review GIT_SECURITY.md for best practices
echo.

echo [OK] All done! Your git security is configured.
echo.
pause
