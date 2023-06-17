using Jvedio.Core.Global;
using Jvedio.Core.Server;
using SuperControls.Style;
using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using static Jvedio.App;
using SuperUtils.NetWork;
using SuperUtils.Windows.WindowCmd;
using System.Threading.Tasks;
using System.Diagnostics;
using SuperControls.Style.Windows;
using SuperUtils.NetWork.Entity;
using System.Collections.Generic;
using Jvedio.Core.DataBase;
using SuperUtils.Common;
using SuperUtils.IO;

namespace Jvedio
{
    /// <summary>
    /// Window_Server.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Server : BaseWindow
    {
        public enum ServerStatus
        {
            UnReady,
            Starting,
            Ready
        }

        public Action<ServerStatus> OnServerStatusChanged;

        private string _LocalIp;
        public string LocalIp
        {
            get { return _LocalIp; }
            set
            {
                _LocalIp = value;
                RaisePropertyChanged();
            }
        }

        private bool _Starting;
        public bool Starting
        {
            get { return _Starting; }
            set
            {
                _Starting = value;
                RaisePropertyChanged();
                if (Starting)
                    CurrentStatus = ServerStatus.Starting;
            }
        }
        private ServerStatus _CurrentStatus;
        public ServerStatus CurrentStatus
        {
            get { return _CurrentStatus; }
            set
            {
                _CurrentStatus = value;
                RaisePropertyChanged();
                OnServerStatusChanged?.Invoke(value);
            }
        }
        private bool _DownLoading;
        public bool DownLoading
        {
            get { return _DownLoading; }
            set
            {
                _DownLoading = value;
                RaisePropertyChanged();
            }
        }
        private int _Progress;
        public int Progress
        {
            get { return _Progress; }
            set
            {
                _Progress = value;
                RaisePropertyChanged();
            }
        }






        public Window_Server()
        {
            InitializeComponent();
            this.DataContext = this;

        }



        private async void BaseWindow_ContentRendered(object sender, System.EventArgs e)
        {
            Init();
            CurrentStatus = await ServerManager.CheckStatus();
        }

        public void Init()
        {
            // 获取本地 IP
            try
            {
                LocalIp = NetUtils.GetLocalIPAddress();
            }
            catch (Exception ex)
            {
                MessageCard.Error(ex.Message);
            }

            Logger.Info($"get local ip: {LocalIp}");
        }



        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private static Process CurrentProcess { get; set; }

        private async void StartServer()
        {
            // 0.将配置写入
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("SqliteDataPath", SqlManager.DEFAULT_SQLITE_PATH);
            dict.Add("SqliteDataConfigPath", SqlManager.DEFAULT_SQLITE_CONFIG_PATH);
            string configString = JsonUtils.TrySerializeObject(dict);
            FileHelper.TryWriteToFile(ServerManager.ServerConfigPath, configString);
            Log("写入配置成功");


            await Task.Delay(300);
            ClearLog();
            Starting = true;
            Log("检查文件...");
            // 1. 检查文件是否存在
            if (!File.Exists(ServerManager.ServerFilePath) && !await ServerManager.DownloadJar())
            {
                MessageCard.Error($"下载文件失败，请前往手动下载：{UrlManager.ServerUrl}");
                Starting = false;
                return;
            }

            Log("文件最新");
            Log("开始启动服务");

            // 2. 启动服务端
            string cmdParams = $"-Xmx2048m -Dserver.port={ConfigManager.JavaServerConfig.Port} -Dloader.path=\"{ServerManager.ServerLibPath}\" -jar \"{ServerManager.ServerFilePath}\"";
            Logger.Info($"run server with arg: {cmdParams}");
            Log(cmdParams);

            bool success = true;
            await Task.Run(() =>
            {
                CmdHelper.Run("java", cmdParams, (msg) =>
                {
                    Log(msg);
                    if (msg.IndexOf("was already in use") >= 0)
                    {
                        success = false;
                        return;
                    }
                    else if (msg.IndexOf("jvedio server start ok!") >= 0)
                    {
                        Starting = false;
                        CurrentStatus = ServerStatus.Ready;
                    }
                }, (err) =>
                {
                    Log(err);
                }, (ex) =>
                {
                    Log(ex.Message);
                    MessageCard.Error(ex.Message);
                }, onCreated: (p) =>
                {
                    CurrentProcess = p;
                });
            });
            if (!success)
            {
                if ((bool)new MsgBox($"端口 {ConfigManager.JavaServerConfig.Port} 被占用，是否关闭对应进程并重启？").ShowDialog(this))
                {
                    success = ProcessManager.KillByPort((int)ConfigManager.JavaServerConfig.Port);
                    if (success)
                    {
                        Log("关闭进程成功！");
                        await Task.Delay(1000);
                        StartServer();
                    }
                    else
                    {
                        MessageNotify.Error($"关闭端口 {ConfigManager.JavaServerConfig.Port} 对应的进程失败");
                    }
                }
            }
        }


        private void StartServer(object sender, System.Windows.RoutedEventArgs e)
        {
            StartServer();
        }

        public void StopServer()
        {

            Log("停止当前服务");
            CurrentProcess?.Close();
            bool success = ProcessManager.KillByPort((int)ConfigManager.JavaServerConfig.Port);
            if (success)
            {
                Log("关闭进程成功");
                CurrentStatus = ServerStatus.UnReady;
            }
            else
                Log("关闭进程失败");

        }

        private void StopServer(object sender, System.Windows.RoutedEventArgs e)
        {
            StopServer();
        }

        private void Log(string msg)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                logTextBox.AppendText(msg + Environment.NewLine);
                Logger.Info(msg);
                logTextBox.ScrollToEnd();
            });
        }
        private void ClearLog()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                logTextBox.Clear();
            });
        }

        private void BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManager.JavaServerConfig.Save();
        }

        private void ShowHelp(object sender, System.Windows.RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(UrlManager.ServerHelpUrl);
        }
    }
}
