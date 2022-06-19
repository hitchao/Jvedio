@echo off
cls
set project_dir="D:\Jvedio\jvedioupdate"
set /p input=please input the project_dir of [jvedioupdate], default is(%project_dir%):
if "%input%" neq "" (set project_dir=%input%)
echo the project_dir of [jvedioupdate] is %project_dir%
rd /s/q %project_dir%\File
del /f/s/q %project_dir%\list
del /f/s/q %project_dir%\list.json
xcopy /y/e .\File %project_dir%\File\
copy /y .\list %project_dir%\list
copy /y .\list.json %project_dir%\list.json
pause
cls
