@COLOR 0A
@PROMPT $S
:: clean up before starting
@DEL *.exe
@DEL *.pdb
@CD ..
@RD bin /s /q
@RD obj /s /q

@ECHO.
@ECHO.
@ECHO  CLEANING COMPLETE!   READY TO START?
@ECHO.
@PAUSE

@CLS
@ECHO.
@ECHO.
@COLOR 0F

:: Launch the Visual Studio Build
@SET VS="%ProgramFiles%\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe"
@IF NOT EXIST %VS% SET VS="%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe"
@IF NOT EXIST %VS% GOTO ERROR
CALL %VS% ..\CodeFetcher.sln /build Release
@ECHO.

:: clean up 
@DEL bin\release\*.config /q
@DEL bin\release\*.xml /q
@DEL bin\release\*.pdb /q
@DEL bin\release\*.dll /q
@MOVE bin\release\CodeFetcher.exe install


@COLOR 0A
@ECHO.
@ECHO.
@ECHO  PROCESS COMPLETE!
@ECHO.
@PAUSE
@EXIT


:ERROR
@COLOR 0C
@ECHO.
@ECHO.
@ECHO  VISUAL STUDIO NOT FOUND!
@ECHO.
@PAUSE
