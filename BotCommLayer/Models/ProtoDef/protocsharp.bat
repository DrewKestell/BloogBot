@echo off
setlocal enabledelayedexpansion

rem Set the default paths if not provided
if "%~1"=="" (
    echo No path to .proto files provided. Using local path
    set "PROTO_DIR=."
) else (
    set "PROTO_DIR=%~1"
)

if "%~2"=="" (
    echo No output directory provided. Using default ../
    set "OUTPUT_DIR=.."
) else (
    set "OUTPUT_DIR=%~2"
)

if "%~3"=="" (
    echo No protoc path provided. Using default
    set "PROTOC_PATH=C:\protoc\bin\protoc.exe"
) else (
    set "PROTOC_PATH=%~3"
)

rem Validate that protoc.exe exists
if not exist "%PROTOC_PATH%" (
    echo ERROR: protoc.exe not found at "%PROTOC_PATH%"
    exit /b 1
)

rem Change the directory to where your .proto files are located
pushd "%PROTO_DIR%" || (
    echo Failed to change directory to %PROTO_DIR%
    exit /b 1
)

rem Create the output directory if it does not exist
if not exist "%OUTPUT_DIR%" (
    mkdir "%OUTPUT_DIR%" || (
        echo Failed to create output directory %OUTPUT_DIR%
        popd
        exit /b 1
    )
)

set "FILES_GENERATED=false"
for %%f in (*.proto) do (
    echo Processing %%f
    "%PROTOC_PATH%" --csharp_out="%OUTPUT_DIR%" -I"." "%%f"
    if !ERRORLEVEL! neq 0 (
        echo Failed to process %%f with protoc
        popd
        exit /b 1
    )
    set "FILES_GENERATED=true"
)

if "!FILES_GENERATED!"=="false" (
    echo No .proto files found or processed.
    popd
    exit /b 1
)

echo C# files generated successfully in "%OUTPUT_DIR%"
popd
endlocal
