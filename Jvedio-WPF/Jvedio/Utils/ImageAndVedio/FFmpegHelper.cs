using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Utils.ImageAndVedio
{
    public class FFmpegHelper
    {
        private string ProcessParameters = "";
        private int Timeout = 0;

        public static int MaxProcessWaitingSecond = 5; //ffmpeg 超时等待时间

        public FFmpegHelper(string processParameters, int timeoutsecond = 0)
        {
            if (!processParameters.EndsWith("&exit")) ProcessParameters = processParameters + "&exit";
            if (timeoutsecond <= 0)
                Timeout = MaxProcessWaitingSecond * 1000;
            else
                Timeout = timeoutsecond * 1000;
        }
        public async Task<string> Run()
        {
            await Task.Delay(1);
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    //process.StartInfo.Arguments = arguments;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                    process.Start();
                    process.StandardInput.WriteLine(ProcessParameters);
                    process.StandardInput.AutoFlush = true;
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit(Timeout);
                    if (process.ExitCode == 0)
                        return "成功";
                    else
                        return "失败";
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
            return "";
        }

    }
}
