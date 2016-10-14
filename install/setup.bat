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


:: Launch the Visual Studio Build
@COLOR 07
@SET VS="%ProgramFiles%\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe"
@IF NOT EXIST %VS% SET VS="%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe"
@IF NOT EXIST %VS% GOTO ERROR
CALL %VS% CodeFetcher.sln /build Release
@ECHO.

:: clean up 
@DEL bin\release\*.config /q
@DEL bin\release\*.xml /q

:: ILMerge
CALL packages\ILMerge.2.14.1208\tools\ILMerge.exe /wildcards /t:winexe /log:CodeFetcher.log /out:CodeFetcher.exe  bin\Release\CodeFetcher.exe  bin\Release\*.dll


@ECHO.
@ECHO.
@COLOR 0A
@MOVE CodeFetcher.exe install
@MOVE CodeFetcher.pdb install
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
