using Jvedio.Core.Scan;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Moq;
using System.Threading.Tasks;
using System;
using SuperUtils.Reflections;

namespace Jvedio.Test.ScanTest
{
    [TestClass]
    public class ImportTest
    {
        // mock 参考 https://learn.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking

        [TestMethod]
        [DataRow(new string[] { "abcd-123.mp4" })]
        public void BasicImport(string[] scanPaths, string[] filePaths)
        {
            Core.Scan.ScanTask scanTask = new Core.Scan.ScanTask(scanPaths.ToList(), filePaths.ToList());
            ScanResult scanResult = scanTask.ScanResult;
            List<string> import = scanResult.Import;
            List<string> failNFO = scanResult.FailNFO;
            long total = scanResult.TotalCount;
            Dictionary<string, ScanDetailInfo> notImport = scanResult.NotImport;
            Assert.IsNotNull(import);
        }

        [TestMethod]
        [DataRow(new string[] {
            "D:\\电影\\abcd-123-1.mp4",
            "D:\\电影\\abcd-123-2.mp4",
            "D:\\电影\\abcd-123-3.mp4",
            "D:\\电影\\abcd-123-4.mp4",
            "D:\\电影\\abcd-123-5.mp4",
            "D:\\电影\\abcd-123-6.mp4",
            "D:\\电影\\abcd-123-7.mp4",
            "D:\\电影\\abcd-123-8.mp4",
            "D:\\电影\\abcd-123-9.mp4",
            "D:\\电影\\abcd-123-10.mp4",
            "D:\\电影\\abcd-123-11.mp4",
            "D:\\电影\\abcd-123-12.mp4" })]
        public void SubSectionImport(string[] filePaths)
        {
            Jvedio.App.Init();
            Mock<IScan> mock = new Mock<IScan>();
            mock.Setup(arg => arg.GetMinFileSize()).Returns(0);
            List<string> list = filePaths.ToList();
            Assert.AreEqual(12, list.Count);

            ScanTask scanTask = new ScanTask(null, list);
            scanTask.Start();
            while (true) {
                Task.Delay(20);
                if (!scanTask.Running)
                    break;
            }
            ScanResult scanResult = scanTask.ScanResult;
            Assert.IsNotNull(scanResult);
            Console.WriteLine(ClassUtils.ToString(scanResult, true));
            //List<string> import = scanResult.Import;
            //List<string> failNFO = scanResult.FailNFO;
            //long total = scanResult.TotalCount;
            //Dictionary<string, ScanDetailInfo> notImport = scanResult.NotImport;
            //Assert.IsNotNull(import);
            //Assert.AreEqual(1, import.Count);
        }
    }
}
