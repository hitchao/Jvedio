using Jvedio.Core.Global;
using Jvedio.Core.Logs;
using Microsoft.VisualBasic.Logging;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.IO;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Jvedio
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public EventWaitHandle ProgramStarted { get; set; }

        static App()
        {
            Window_ErrorMsg.OnFeedBack += () =>
            {
                FileHelper.TryOpenUrl(UrlManager.FeedBackUrl);
            };
            Window_ErrorMsg.OnLog += (str) =>
            {
                Logger.Info(str);
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            Console.WriteLine("***************OnStartup***************");
            Console.WriteLine("Debug 不捕获未处理异常");
#else
            bool createNew;
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "Jvedio", out createNew);
            if (!createNew)
            {
                new MsgBox(null, $"Jvedio {LangManager.GetValueByKey("Running")}").ShowDialog();
                App.Current.Shutdown();
                Environment.Exit(0);
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

        protected override void OnExit(ExitEventArgs e)
        {
            Console.WriteLine("***************OnExit***************");
            base.OnExit(e);
        }
    }
}
