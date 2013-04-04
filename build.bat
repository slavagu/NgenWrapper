@echo off
pushd %~dp0
if "%WindowsSdkDir%" == "" call "%VS100COMNTOOLS%\vsvars32.bat"

if "%BUILD_CONFIG%" == "" set BUILD_CONFIG=Debug

echo Building %BUILD_CONFIG%...
msbuild.exe NgenWrapper.sln /t:Rebuild /p:Configuration=%BUILD_CONFIG%

echo Running tests...
packages\NUnit.Runners.2.6.2\tools\nunit-console.exe "NgenWrapper.Tests\bin\%BUILD_CONFIG%\NgenWrapper.Tests.dll" /framework=net-4.0 /labels

echo Done.
popd

