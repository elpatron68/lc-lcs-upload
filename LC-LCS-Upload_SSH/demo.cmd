@echo off
rem Demo batch file for mass processing
rem ===================================
rem
rem This file has to be executed from the directory where the script files are located.
rem If 'LC-LCS-Upload_SSH.exe' is located in another directory, the path has to be added.
rem
rem Otherwise, the directory of LC-LCS-Upload_SSH could be added to the global PATH
rem environment variable.
rem
rem Setting defaults with environment variables
rem ===========================================
rem SET LC-LCS-ADDRESS=mylancom.foo.bar
rem SET LC-LCS-USERNAME=admin
rem SET LC-LCS-PASSWORD=pA55w0Rd
rem
rem Example usage (comment out one of them)
rem =============
rem Apply scripts, create no backup
for /r %%A in (*.lcs) do LC-LCS-Upload_SSH.exe -i %%A
rem
rem Apply scripts, create a backup of each device
for /r %%A in (*.lcs) do LC-LCS-Upload_SSH.exe -b backup -i %%A

