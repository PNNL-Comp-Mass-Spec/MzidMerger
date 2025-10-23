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

xcopy Debug\net48\MzidMerger.exe \\Proto-3\DMS_Programs_Dist\AnalysisToolManagerDistribution\MzidMerger\ /D /Y
xcopy Debug\net48\MzidMerger.pdb \\Proto-3\DMS_Programs_Dist\AnalysisToolManagerDistribution\MzidMerger\ /D /Y
xcopy Debug\net48\*.dll          \\Proto-3\DMS_Programs_Dist\AnalysisToolManagerDistribution\MzidMerger\ /D /Y
xcopy ..\..\Readme.md            \\Proto-3\DMS_Programs_Dist\AnalysisToolManagerDistribution\MzidMerger\ /D /Y

pause
