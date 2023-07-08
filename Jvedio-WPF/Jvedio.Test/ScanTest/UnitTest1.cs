using Jvedio.Core.Scan;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Jvedio.Test.ScanTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [DataRow(new string[] { "abcd-123.mp4" })]
        public void TestMethod1(string[] scanPaths, string[] filePaths)
        {
            Core.Scan.ScanTask scanTask = new Core.Scan.ScanTask(scanPaths.ToList(), filePaths.ToList());
            ScanResult scanResult = scanTask.ScanResult;
            List<string> import = scanResult.Import;
            List<string> failNFO = scanResult.FailNFO;
            long total = scanResult.TotalCount;
            Dictionary<string, ScanDetailInfo> notImport = scanResult.NotImport;
            Assert.IsNotNull(import);
        }
    }
}
