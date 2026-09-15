@echo off
setlocal

set "ROOT=%~dp0.."
pushd "%ROOT%"

dotnet build Tools/DataTableGenerator/DataTableGenerator.csproj -c Release
if errorlevel 1 goto :error

echo === A. Excel -^> Text ===
dotnet run --project Tools/DataTableGenerator --no-build -c Release -- excel-all --root "%ROOT%"
if errorlevel 1 goto :error

echo.
echo === B. Text -^> Bytes + Code ===
dotnet run --project Tools/DataTableGenerator --no-build -c Release -- generate --root "%ROOT%"
if errorlevel 1 goto :error

echo.
echo Done.
popd
exit /b 0

:error
popd
exit /b 1
