xcopy Debug\net48\MzidMerger.exe C:\DMS_Programs\MzidMerger\ /D /Y
xcopy Debug\net48\MzidMerger.pdb C:\DMS_Programs\MzidMerger\ /D /Y
xcopy Debug\net48\*.dll          C:\DMS_Programs\MzidMerger\ /D /Y
xcopy ..\..\Readme.md            C:\DMS_Programs\MzidMerger\ /D /Y

@echo off
echo.
echo.
echo About to copy to AnalysisToolManagerDistribution
echo.
pause
@echo on

xcopy Debug\net48\MzidMerger.exe \\pnl\projects\OmicsSW\DMS_Programs\AnalysisToolManagerDistribution\MzidMerger\ /D /Y
xcopy Debug\net48\MzidMerger.pdb \\pnl\projects\OmicsSW\DMS_Programs\AnalysisToolManagerDistribution\MzidMerger\ /D /Y
xcopy Debug\net48\*.dll          \\pnl\projects\OmicsSW\DMS_Programs\AnalysisToolManagerDistribution\MzidMerger\ /D /Y
xcopy ..\..\Readme.md            \\pnl\projects\OmicsSW\DMS_Programs\AnalysisToolManagerDistribution\MzidMerger\ /D /Y

pause
