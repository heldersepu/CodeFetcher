@COLOR 0A
@PROMPT $S
:: clean up before starting
@DEL *.exe
@CD ..
@RD bin /s /q
@RD obj /s /q

@ECHO.
@ECHO.
@ECHO  CLEANING COMPLETE!   READY TO START?
@ECHO.
@PAUSE


:: Launch the Visual Studio Build
@COLOR 07
@SET VS="%ProgramFiles%\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe"
@IF NOT EXIST %VS% SET VS="%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe"
CALL %VS% CodeFetcher.sln /build Release
@ECHO.

:: clean up at the end
@CD bin
@CD release
@DEL *.config /q
@DEL *.xml /q
@DEL *.pdb /q

@ECHO.
@ECHO.
@COLOR 0A
@MOVE *.exe ../../install
@ECHO.
@PAUSE
