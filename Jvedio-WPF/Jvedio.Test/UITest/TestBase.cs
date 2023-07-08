using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using System;
using System.Diagnostics;
using System.IO;

namespace Jvedio.Test.UITest
{
    public class TestBase
    {
        private const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        private static string ApplicationPath = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Jvedio\bin\Debug\Jvedio.exe"));
        private const string DeviceName = "WindowsPC";
        private const int WaitForAppLaunch = 1;
        private const string WinAppDriverPath = @"C:\Program Files\Windows Application Driver\WinAppDriver.exe";
        private static Process winAppDriverProcess;
        public WindowsDriver<WindowsElement> AppSession { get; private set; }
        public WindowsDriver<WindowsElement> DesktopSession { get; private set; }


        public const int SLEEP_SLOW = 200;
        public const int SLEEP_MEDIUM = 500;
        public const int SLEEP_HIGH = 800;
        public const int SLEEP_1S = 1000;
        public const int SLEEP_2S = 2000;
        public const int SLEEP_5S = 5000;
        public const int SLEEP_10S = 10 * 1000;

        public const string WINDOW_SKIN_LANG = "SkinLangWindow";
        public const string DIALOG_CONFIRM = "Dialog_Confirm";
        public const string DIALOG_CANCEL = "Dialog_Cancel";
        public const string SKIN_BLACK = "Black";
        public const string SKIN_WHITE = "White";

        public TestBase()
        {
#if DEBUG
            ApplicationPath = Path.GetFullPath(
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Jvedio\bin\Debug\Jvedio.exe"));
#else
            ApplicationPath = Path.GetFullPath(
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Jvedio\bin\Release\Jvedio.exe"));
#endif
        }

        public string GetBaseDir()
        {
            return Path.GetDirectoryName(ApplicationPath);
        }

        public string GetDataDir()
        {
            return Path.Combine(GetBaseDir(), "data");
        }



        private static void StartWinAppDriver()
        {
            ProcessStartInfo psi = new ProcessStartInfo(WinAppDriverPath);
            psi.UseShellExecute = true;
            psi.Verb = "runas"; // run as administrator
            winAppDriverProcess = Process.Start(psi);
        }

        public void Initialize()
        {
            StartWinAppDriver();
            var appiumOptions = new AppiumOptions();
            appiumOptions.AddAdditionalCapability("app", ApplicationPath);
            appiumOptions.AddAdditionalCapability("deviceName", DeviceName);
            appiumOptions.AddAdditionalCapability("ms:waitForAppLaunch", WaitForAppLaunch);
            this.AppSession =
                new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appiumOptions);
            Assert.IsNotNull(AppSession);
            Assert.IsNotNull(AppSession.SessionId);
            AppSession.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.5);
            AppiumOptions optionsDesktop = new AppiumOptions();
            optionsDesktop.AddAdditionalCapability("app", "Root");
            optionsDesktop.AddAdditionalCapability("deviceName", DeviceName);
            DesktopSession = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), optionsDesktop);
        }

        public void Cleanup()
        {
            // Close the session
            if (AppSession != null) {
                AppSession.Close();
                AppSession.Quit();
            }
            // Close the desktopSession
            if (DesktopSession != null) {
                DesktopSession.Close();
                DesktopSession.Quit();
            }
        }
        public static void StopWinappDriver()
        {
            // Stop the WinAppDriverProcess
            if (winAppDriverProcess != null) {
                foreach (var process in Process.GetProcessesByName("WinAppDriver")) {
                    process.Kill();
                }
            }
        }


        protected void SelectAllText()
        {
            Actions action = new Actions(AppSession);
            action.KeyDown(Keys.Control).SendKeys("a");
            action.KeyUp(Keys.Control);
            action.Perform();
        }

        protected void AltF4()
        {
            Actions action = new Actions(AppSession);
            action.KeyDown(Keys.LeftAlt).SendKeys("F4");
            action.KeyUp(Keys.Control);
            action.Perform();
        }

        protected void PerformDelete()
        {
            Actions action = new Actions(AppSession);
            action.SendKeys(Keys.Delete);
            action.Perform();
        }

        protected void PerformEnter()
        {
            Actions action = new Actions(AppSession);
            action.SendKeys(Keys.Enter);
            action.Perform();
        }

        protected void WriteText(string text)
        {
            Actions action = new Actions(AppSession);
            action.SendKeys(text);
            action.Perform();
        }

        public WindowsElement FindByName(string name)
        {
            return this.AppSession.FindElementByName(name);
        }
        public WindowsElement FindById(string id)
        {
            return this.AppSession.FindElementByAccessibilityId(id);
        }

        public bool ClickById(string id)
        {
            WindowsElement btn = this.FindById(id);
            if (btn == null)
                return false;
            btn.Click();
            return true;
        }

        public bool ClickByXPath(string xpath)
        {
            var ele = this.AppSession.FindElementByXPath(xpath);
            if (ele == null)
                return false;
            ele.Click();
            return true;
        }

        public bool ClickByText(string text)
        {
            return false;
        }
    }
}
