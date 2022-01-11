@echo off
cls
set p="D:\Jvedio\Jvedio"
:start
echo 进入的路径为 %p%
echo 【-----请执行操作-----】
echo      1:commit
echo      2:push
echo      3:commit + push
echo      4:pull
echo      5:exit
echo 【-----请执行操作-----】
set /p choice=请输入：

if %choice% ==1 (
	echo 执行命令：commit
	cd %p% && git add --all && git commit -m "jvedio"
) else if %choice% ==2 (
	echo 执行命令：push
	cd %p% && git push
) else if %choice% ==3  (
	echo 执行命令：commit + push
	cd %p% && git add --all && git commit -m "jvedio" && git push
) else if %choice% ==4  (
	echo 执行命令：pull
	cd %p% && git pull
) else if %choice% ==5  (
	exit
) else (
	echo 命令输入有误，请重新输入
)
pause
cls
goto start
