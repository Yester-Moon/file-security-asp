@echo off
REM Pre-commit hook for Windows
REM Copy this file to .git/hooks/pre-commit (without .bat extension)

echo Running pre-commit security checks...

REM Check for sensitive files
git diff --cached --name-only | findstr /I "appsettings.json" > nul
if %errorlevel% equ 0 (
    echo ERROR: Cannot commit appsettings.json
    echo Please use appsettings.*.json.example template files
    exit /b 1
)

git diff --cached --name-only | findstr /I "appsettings.Development.json" > nul
if %errorlevel% equ 0 (
    echo ERROR: Cannot commit appsettings.Development.json
    exit /b 1
)

git diff --cached --name-only | findstr /I "appsettings.Production.json" > nul
if %errorlevel% equ 0 (
    echo ERROR: Cannot commit appsettings.Production.json
    exit /b 1
)

git diff --cached --name-only | findstr /I ".env" > nul
if %errorlevel% equ 0 (
    echo ERROR: Cannot commit .env files
    exit /b 1
)

git diff --cached --name-only | findstr /I ".pfx" > nul
if %errorlevel% equ 0 (
    echo ERROR: Cannot commit certificate files
    exit /b 1
)

REM Check for hardcoded secrets
git diff --cached | findstr /I /R "password.*=.*['\"].*['\"]" > nul
if %errorlevel% equ 0 (
    echo WARNING: Detected possible hardcoded password
    set /p "response=Continue anyway? (y/N): "
    if /I not "%response%"=="y" (
        echo Commit cancelled
        exit /b 1
    )
)

echo Security checks passed!
exit /b 0
