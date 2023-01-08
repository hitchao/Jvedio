using Jvedio.Core.Logs;
using SuperControls.Style;
using SuperControls.Style.Windows;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            Console.WriteLine("***************OnStartup***************");
            bool createNew;
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "Jvedio", out createNew);
            if (!createNew)
            {
                new MsgBox(null, $"Jvedio {LangManager.GetValueByKey("Running")}").ShowDialog();
                App.Current.Shutdown();
                Environment.Exit(0);
            }

#if DEBUG
            Console.WriteLine("Debug 不捕获未处理异常");
#else
            // UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);

            // Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // 非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

#endif
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Console.WriteLine("***************OnExit***************");
            base.OnExit(e);
        }



        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Window_ErrorMsg window_ErrorMsg = new Window_ErrorMsg();
                window_ErrorMsg.SetError(e.Exception.ToString());
                window_ErrorMsg.ShowDialog();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                App.Current.Shutdown();
                Environment.Exit(0);
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            try
            {
                StringBuilder builder = new StringBuilder();
                if (e.IsTerminating)
                {
                    Window_ErrorMsg window_ErrorMsg = new Window_ErrorMsg();
                    window_ErrorMsg.SetError(e.ExceptionObject.ToString());
                    window_ErrorMsg.ShowDialog();
                    Logger.Warning(builder.ToString());
                    if (e.ExceptionObject is Exception ex)
                    {

                        Logger.Error(ex);
                    }
                    else
                    {
                        builder.Append(e.ExceptionObject);
                        Logger.Warning(e.ExceptionObject.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                App.Current.Shutdown();
                Environment.Exit(0);
            }
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // task线程内未处理捕获
            Console.WriteLine(e.Exception.StackTrace);
            Console.WriteLine(e.Exception.Message);
            Logger.Error(e.Exception);
            e.SetObserved(); // 设置该异常已察觉（这样处理后就不会引起程序崩溃）
        }
    }
}
