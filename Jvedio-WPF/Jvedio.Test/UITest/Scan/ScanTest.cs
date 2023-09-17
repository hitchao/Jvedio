using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium.Windows;
using System.Threading;

namespace Jvedio.Test.UITest
{
    [TestClass]
    [Ignore]
    public class UnitTest : TestBase
    {
        private const string BUTTON_NEW_DATA_BASE = "newDataBaseButton";
        [TestInitialize]
        public void TestInitialize()
        {
            this.Initialize();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.Cleanup();
        }

        [ClassCleanup]
        public static void ClassCleanusp()
        {
            StopWinappDriver();
        }

        string[] X_PATH_LIST = {
            "/Window[@Name=\"欢迎使用 Jvedio\"][@AutomationId=\"Window_StartUp\"]/Window[@Name=\"选择\"][@AutomationId=\"SkinLangWindow\"]/RadioButton[@ClassName=\"RadioButton\"]",
            "/Window[@Name=\"欢迎使用 Jvedio\"][@AutomationId=\"Window_StartUp\"]/Window[@Name=\"选择\"][@AutomationId=\"SkinLangWindow\"]/Button[@ClassName=\"Button\"]",
        };


        [TestMethod]

        [DataRow("测试扫描")]
        public void CreateDataBase(string dbName)
        {
            WindowsElement firstRunWindow = this.FindById(WINDOW_SKIN_LANG);
            if (firstRunWindow != null) {
                Assert.IsTrue(this.ClickByXPath(X_PATH_LIST[0]));
                Assert.IsTrue(this.ClickByXPath(X_PATH_LIST[1])); // 关闭
            }

            Assert.IsTrue(this.ClickById(BUTTON_NEW_DATA_BASE));
            Thread.Sleep(SLEEP_SLOW);
            this.WriteText(dbName);
            this.PerformEnter();
            Thread.Sleep(SLEEP_1S);

            // 点击进入该库
            //this.ClickById


        }


    }
}
