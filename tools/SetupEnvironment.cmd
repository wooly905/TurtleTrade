@echo off

rem :: Admin check - TurtleEnlistment.cmd must be run as an administrator
rem net session >nul 2>&1
rem if %errorlevel% NEQ 0 (
rem     echo This command must be run as an administrator
rem     exit /b 1
rem )

set "turtleRoot=%~dp0"
set "turtleRoot=%turtleRoot:~0,-7%"
set "turtleTools=%turtleRoot%\tools"
set "turtleSource=%turtleRoot%\src"
set "turtleTest=%turtleRoot%\test"
SET "PATH=%PATH%;%turtleTools%;"
set "IsDev=yes"

alias.exe -f "%turtleTools%\alias.txt"

cd %turtleRoot%
dotnet restore

echo This is Turtle dev environment
