@REM if msbuild command is recognized go to build, otherwise setup build vars

@set buildType=Debug
if  "%1" == "release" @set buildType=Release
@set logFileType=%buildType%
if "%2" == "/t:clean" @set logFileType=clean

@where /q msbuild && goto build
@goto set_build_vars



:set_build_vars
@echo setting up the build vars...
@set _programFiles=%ProgramFiles%
@if defined ProgramFiles(x86) set _programFiles=%ProgramFiles(x86)%
@REM first, check if visual studio 2022 exist and build with it
@set _msbuild_location=%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin
@if exist "%_msbuild_location%" @set PATH=%_msbuild_location%;%PATH% && goto build
@set _msbuild_location=%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin
@if exist "%_msbuild_location%" @set PATH=%_msbuild_location%;%PATH% && goto build
@set _msbuild_location=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin
@if exist "%_msbuild_location%" @set PATH=%_msbuild_location%;%PATH% && goto build
@REM first, check if visual studio 2019 exist and build with it
@set _msbuild_location=%_programFiles%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\
@if exist "%_msbuild_location%" @set PATH=%_msbuild_location%;%PATH% && goto build
@set _msbuild_location=%_programFiles%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\
@if exist "%_msbuild_location%" @set PATH=%_msbuild_location%;%PATH% && goto build
@set _msbuild_location=%_programFiles%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\
@if exist "%_msbuild_location%" @set PATH=%_msbuild_location%;%PATH% && goto build
@REM second, check if MSBuild exist and build with it
@set _msbuild_location=%_programFiles%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\
@if exist "%_msbuild_location%" @set PATH=%_msbuild_location%;%PATH% && goto build
@REM first, check if visual studio 2017 exist and build with it
@set _msbuild_location=%_programFiles%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\
@if exist "%_msbuild_location%" @set PATH=%_msbuild_location%;%PATH% && goto build

@set _msbuild_location=%_programFiles%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\
@if exist "%_msbuild_location%" @set PATH=%_msbuild_location%;%PATH% && goto build

@set _msbuild_location=%_programFiles%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\
@if exist "%_msbuild_location%" @set PATH=%_msbuild_location%;%PATH% && goto build

@REM second, check if MSBuild exist and build with it
@set _msbuild_location=%_programFiles%\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\
@if exist "%_msbuild_location%" @set PATH=%_msbuild_location%;%PATH% && goto build

@set _msbuild_location=%_programFiles%\MSBuild\14.0\bin\
@if exist "%_msbuild_location%" @set PATH=%_msbuild_location%;%PATH% && goto build

@REM last, check if any older version of visual studio exist and build with it
@if exist "%VS140COMNTOOLS%" @call "%VS140COMNTOOLS%\vsvars32.bat" && goto build
@if exist "%VS120COMNTOOLS%" @call "%VS120COMNTOOLS%\vsvars32.bat" && goto build
@if exist "%VS110COMNTOOLS%" @call "%VS110COMNTOOLS%\vsvars32.bat" && goto build
@if exist "%VS100COMNTOOLS%" @call "%VS100COMNTOOLS%\vsvars32.bat" && goto build

@REM error -  not found
@echo  ERROR: Cannot determine the location of MSBuild. Please make sure Visual Studio or MSBuild is installed.
@exit /B 1

:build
del build%logFileType%.log
del bin\*.* /q
@echo ***************************************************
@echo *                                                 *
@echo *  Building - Please DO NOT close the window...   *
@echo *                                                 *
@echo ***************************************************
@echo BuildStart-%time% >>buildDebug.log
msbuild msbuild.xml /clp:ErrorsOnly /filelogger1 /fileloggerparameters1:logfile=build%logFileType%.log;append;verbosity=n;errorsonly /p:Configuration=%buildType% /p:Platform=AnyCPU  /maxcpucount:%NUMBER_OF_PROCESSORS% %2
@echo BuildEnd-%time% >>buildDebug.log

if not "%2" == "/t:clean" BuildAnalyzer.exe build%logFileType%.log
