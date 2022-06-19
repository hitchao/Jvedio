@echo off
cls
set p="D:\Jvedio\Jvedio"
:start
echo run command in dir: %p%
echo [-----choice-----]
echo      1:commit
echo      2:push
echo      3:commit + push
echo      4:pull
echo      5:exit
echo [-----choice-----]
set /p choice=enter : 

if %choice% ==1 (
	echo run command : commit
	cd %p% && git add --all && git commit -m "jvedio-default-commit"
) else if %choice% ==2 (
	echo run command : push
	cd %p% && git push
) else if %choice% ==3  (
	echo run command : commit + push
	cd %p% && git add --all && git commit -m "jvedio-default-commit" && git push
) else if %choice% ==4  (
	echo run command : pull
	cd %p% && git pull
) else if %choice% ==5  (
	exit
) else (
	echo input error, please input again.
)
pause
cls
goto start
