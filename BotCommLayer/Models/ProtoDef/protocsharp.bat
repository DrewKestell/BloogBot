@echo off
setlocal

rem Set the default paths if not provided
if "%~1"=="" (
    echo No path to .proto files provided. Using local path
    set "PROTO_DIR=./"
) else (
    set "PROTO_DIR=%~1"
)

if "%~2"=="" (
    echo No output directory provided. Using default ../
    set "OUTPUT_DIR=../"
) else (
    set "OUTPUT_DIR=%~2"
)

if "%~3"=="" (
    echo No protoc path provided. Using default ../../BotCommLayer/vcpkg_installed/x64-windows/tools/protobuf/protoc
    set "PROTOC_PATH=C:/protoc/bin/protoc"
) else (
    set "PROTOC_PATH=%~3"
)

rem Change the directory to where your .proto files are located
cd "%PROTO_DIR%"
if %ERRORLEVEL% neq 0 (
    echo Failed to change directory to %PROTO_DIR%
    exit /b 1
)

rem Create the output directory if it does not exist
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
if %ERRORLEVEL% neq 0 (
    echo Failed to create output directory %OUTPUT_DIR%
    exit /b 1
)

rem Run protoc to generate C++ files for all .proto files in the directory
set "FILES_GENERATED=false"
for %%f in (*.proto) do (
    echo Processing %%f
    "%PROTOC_PATH%" --csharp_out="%OUTPUT_DIR%" -I="%PROTO_DIR%" %%f
    if %ERRORLEVEL% neq 0 (
        echo Failed to process %%f with protoc
        exit /b 1
    )
    set "FILES_GENERATED=true"
)

rem Check if files were generated successfully
if "%FILES_GENERATED%"=="false" (
    echo No .proto files found or processed.
    exit /b 1
)

echo C# files generated successfully in "%OUTPUT_DIR%"
endlocal
