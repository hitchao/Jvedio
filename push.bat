@echo off
cls
set p="D:\Jvedio\Jvedio"
:start
echo �����·��Ϊ %p%
echo ��-----��ִ�в���-----��
echo      1:commit
echo      2:push
echo      3:commit + push
echo      4:pull
echo      5:exit
echo ��-----��ִ�в���-----��
set /p choice=�����룺

if %choice% ==1 (
	echo ִ�����commit
	cd %p% && git add --all && git commit -m "jvedio"
) else if %choice% ==2 (
	echo ִ�����push
	cd %p% && git push
) else if %choice% ==3  (
	echo ִ�����commit + push
	cd %p% && git add --all && git commit -m "jvedio" && git push
) else if %choice% ==4  (
	echo ִ�����pull
	cd %p% && git pull
) else if %choice% ==5  (
	exit
) else (
	echo ����������������������
)
pause
cls
goto start
