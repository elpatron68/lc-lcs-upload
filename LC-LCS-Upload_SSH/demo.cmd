@echo off
rem This file has to be executed from the directory where the script files are located.
rem If 'LC-LCS-Upload_SSH.exe' is located in another directory, the path has to be added.
rem
rem Otherwise, the directory of LC-LCS-Upload_SSH could be added to the global PATH
rem environment variable.
rem
rem
rem Example usage (comment out one of them)
rem =============
rem Apply scripts, create no backup
for /r %%A in (*.lcs) do LC-LCS-Upload_SSH.exe -i %%A
rem
rem Apply scripts, create a backup of each device
for /r %%A in (*.lcs) do LC-LCS-Upload_SSH.exe -b backup -i %%A
