
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using HandyControl.Tools;
using Jvedio;

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
                MessageBox.Show(Jvedio.Language.Resources.JvedioIsRunning);
                App.Current.Shutdown();
                Environment.Exit(0);
            }


            //UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            //Task线程内未捕获异常处理事件　　　　　
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            SetLanguageDictionary();
            base.OnStartup(e);
        }


        protected override void OnExit(ExitEventArgs e)
        {
            Console.WriteLine("***************OnExit***************");
            base.OnExit(e);
        }




        private void SetLanguageDictionary()
        {
            //设置语言
            long language = Jvedio.Properties.Settings.Default.SelectedLanguage;
            string lang = "en-US";
            switch (language)
            {

                case 0:
                    lang = "zh-CN";
                    break;
                case 1:
                    lang = "en-US";
                    break;
                case 2:
                    lang = "ja-JP";
                    break;
                default:
                    //根据地区获取语言
                    string name = System.Globalization.CultureInfo.CurrentCulture.Name.ToUpper();
                    if (name == "ja-JP".ToUpper())
                    {
                        lang = "ja-JP";
                        Jvedio.Properties.Settings.Default.SelectedLanguage = 2;
                    }
                    else if (name == "zh-CN".ToUpper())
                    {
                        lang = "zh-CN";
                        Jvedio.Properties.Settings.Default.SelectedLanguage = 0;
                    }
                    else if (name == "en-US".ToUpper())
                    {
                        lang = "en-US";
                        Jvedio.Properties.Settings.Default.SelectedLanguage = 1;
                    }
                    else
                    {
                        lang = "en-US";
                        Jvedio.Properties.Settings.Default.SelectedLanguage = 1;
                    }
                    break;
            }
            Jvedio.Properties.Settings.Default.Save();
            Jvedio.Language.Resources.Culture = new System.Globalization.CultureInfo(lang);
            ConfigHelper.Instance.SetLang(lang);//设置 handycontrol 的语言
        }


        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                //e.Handled = true; //把 Handled 属性设为true，表示此异常已处理，程序可以继续运行，不会强制退出      
                Console.WriteLine(e.Exception.StackTrace);
                Console.WriteLine(e.Exception.Message);
                Logger.LogE(e.Exception);
            }
            catch
            {
                //此时程序出现严重异常，将强制结束退出
                Console.WriteLine(e.Exception.StackTrace);
                Console.WriteLine(e.Exception.Message);
                MessageBox.Show(Jvedio.Language.Resources.ErrorToShutdown);
            }

        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //StringBuilder sbEx = new StringBuilder();
            //if (e.IsTerminating)
            //{
            //    sbEx.Append("程序发生致命错误，将终止，请联系运营商！\n");
            //}
            //sbEx.Append("捕获未处理异常：");
            //if (e.ExceptionObject is Exception)
            //{
            //    sbEx.Append(((Exception)e.ExceptionObject).Message);
            //}
            //else
            //{
            //    sbEx.Append(e.ExceptionObject);
            //}
            Console.WriteLine(((Exception)e.ExceptionObject).StackTrace);
            Console.WriteLine(((Exception)e.ExceptionObject).Message);
            Logger.LogE((Exception)e.ExceptionObject);

            Console.WriteLine(((Exception)e.ExceptionObject).Message);
            Console.WriteLine(((Exception)e.ExceptionObject).StackTrace);
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            //task线程内未处理捕获
            Console.WriteLine(e.Exception.StackTrace);
            Console.WriteLine(e.Exception.Message);
            Logger.LogE(e.Exception);
            e.SetObserved();//设置该异常已察觉（这样处理后就不会引起程序崩溃）
        }



    }
}
