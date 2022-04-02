using Jvedio.Utils;
using Jvedio.Utils.FileProcess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Test
{
    public class TestUtils
    {
        public static void TestScanFiles()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            string dir = @"D:\";
            List<Exception> exceptions = new List<Exception>();
            IEnumerable<string> enumerable = DirHelper.GetFileList("*.*", dir, (ex) =>
            {
                exceptions.Add(ex);
            });
            List<string> list = enumerable.ToList();
            Console.WriteLine("扫描目录：" + dir);
            Console.WriteLine($"扫描用时：{watch.ElapsedMilliseconds} ms");
            Console.WriteLine("成功扫描的文件数目：" + list.Count);
            Console.WriteLine("总文件数目：" + (list.Count + exceptions.Count));
            Console.WriteLine("扫描出错的文件/文件夹：\n" + String.Join(Environment.NewLine, exceptions.Select(arg => arg.Message).ToList()));
            watch.Stop();
        }
    }
}
