@echo off

call dotnet tool install -g msharp-build
call msharp-build -notools

if ERRORLEVEL 1 (    
	echo ##################################    
    set /p cont= Error occured. Press Enter to exit.
    exit /b -1
)