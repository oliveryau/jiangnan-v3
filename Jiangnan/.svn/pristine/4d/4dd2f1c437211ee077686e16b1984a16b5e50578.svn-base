@echo off
set WORKSPACE=%~dp0..
set LUBAN_DLL=%WORKSPACE%\Tools\Luban\Luban.dll
set CONF_ROOT=%~dp0

dotnet "%LUBAN_DLL%" ^
    -t client ^
    -c cs-simple-json ^
    -d json ^
    --conf "%CONF_ROOT%\luban.conf" ^
    -x outputCodeDir="%WORKSPACE%\Assets\Res\Scripts\LubanCode" ^
    -x outputDataDir="%WORKSPACE%\Assets\Res\Resources\Config"

pause
