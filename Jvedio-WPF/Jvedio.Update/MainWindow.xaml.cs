
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Jvedio.Update
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string TEMP_DIR = "TEMP";
        private static int DELAY = 5;
        private static int DELAY_RUN_JVEDIO = 1;
        public MainWindow()
        {
            InitializeComponent();
        }


        private void AppendLog(string text)
        {
            textBox.AppendText(text);
            textBox.AppendText(Environment.NewLine);
        }


        private async void BaseWindow_ContentRendered(object sender, EventArgs e)
        {
            await MovieFileAsync();
        }


        private async Task MovieFileAsync()
        {
            textBox.Clear();
            AppendLog($"{DELAY} s 后开始更新");
            await Task.Delay(DELAY * 1000);
            // 复制 TEMP 目录覆盖
            if (!Directory.Exists(TEMP_DIR))
            {
                AppendLog("不存在文件夹：TEMP");
                return;
            }
            else
            {
                AppendLog("开始复制 TEMP 目录");
                bool success = TryCopy(TEMP_DIR, AppDomain.CurrentDomain.BaseDirectory, (err) =>
                  {
                      AppendLog(err);
                  }, (msg) =>
                  {
                      AppendLog(msg);
                  });
                if (success)
                {
                    AppendLog("成功！");
                    // 清理目录
                    try
                    {
                        AppendLog($"删除目录：{TEMP_DIR}");
                        Directory.Delete(TEMP_DIR, true);
                    }
                    catch (Exception ex)
                    {
                        AppendLog(ex.Message);
                    }
                    Process.Start("Jvedio.exe");
                    await Task.Delay(DELAY_RUN_JVEDIO * 1000);
                    Application.Current.Shutdown();
                }
                else
                {
                    AppendLog("错误！");
                }
            }
        }

        public static bool TryCopy(string sourcePath, string targetPath, Action<string> errorBack = null, Action<string> doWork = null)
        {
            try
            {
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    doWork?.Invoke($"创建文件夹：{dirPath}");
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                }

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    string target = newPath.Replace(sourcePath, targetPath);
                    doWork?.Invoke($"复制文件：{newPath} 到 {target}");
                    File.Copy(newPath, target, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                errorBack?.Invoke(ex.Message);
            }
            return false;

        }
    }
}
