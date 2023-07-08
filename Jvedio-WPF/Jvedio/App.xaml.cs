using Jvedio.Core.Global;
using SuperControls.Style.Windows;
using SuperUtils.IO;
using System;
using System.Threading;
using System.Windows;

namespace Jvedio
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {


        public static Jvedio.Core.Logs.Logger Logger = Jvedio.Core.Logs.Logger.Instance;

        public EventWaitHandle ProgramStarted { get; set; }

        static App()
        {
            Window_ErrorMsg.OnFeedBack += () => {
                FileHelper.TryOpenUrl(UrlManager.FeedBackUrl);
            };
            Window_ErrorMsg.OnLog += (str) => {
                Logger.Info(str);
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            SuperUtils.Handler.LogHandler.Logger = Logger;
            SuperControls.Style.Handler.LogHandler.Logger = Logger;
#if DEBUG
            Console.WriteLine("***************OnStartup***************");
            Console.WriteLine("Debug 不捕获未处理异常");
#else
            bool createNew;
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "Jvedio", out createNew);
            if (!createNew)
            {
                //new MsgBox($"Jvedio {LangManager.GetValueByKey("Running")}").ShowDialog();
                //App.Current.Shutdown();
                //Environment.Exit(0);

                var current = Process.GetCurrentProcess();

                foreach (var process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id != current.Id)
                    {
                        IntPtr hWnd = IntPtr.Zero;
                        hWnd = process.MainWindowHandle;

                        // todo 最小化到任务栏后，无法打开窗体
                        // Win32Helper.SendArgs(hWnd, Win32Helper.WIN_CUSTOM_MSG_OPEN_WINDOW);

                        SuperUtils.Systems.Win32Helper.ShowWindowAsync(new HandleRef(null, hWnd), SuperUtils.Systems.Win32Helper.SW_RESTORE);
                        SuperUtils.Systems.Win32Helper.SetForegroundWindow(process.MainWindowHandle);



                        break;
                    }
                }
                Shutdown();
            }

            // UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Window_ErrorMsg.App_DispatcherUnhandledException);

            // Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += Window_ErrorMsg.TaskScheduler_UnobservedTaskException;

            // 非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Window_ErrorMsg.CurrentDomain_UnhandledException);

#endif
            base.OnStartup(e);
        }


        private void SaveConfig()
        {
            Jvedio.Properties.Settings.Default.EditMode = false;
            Jvedio.Properties.Settings.Default.ActorEditMode = false;
            Jvedio.Properties.Settings.Default.Save();
            ConfigManager.SaveAll();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Console.WriteLine("***************OnExit***************");
            SaveConfig();
            base.OnExit(e);
        }




    }
}
