using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Jvedio.Core.DataBase;
using Jvedio.Core.Global;
using Jvedio.Core.Server;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.IO;
using SuperUtils.NetWork;
using SuperUtils.Windows.WindowCmd;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static Jvedio.App;

namespace Jvedio
{
    /// <summary>
    /// Window_Server.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Server : BaseWindow
    {

        #region "事件"
        public Action<ServerStatus> OnServerStatusChanged;


        #endregion

        #region "属性"
        private static StringBuilder LogCache { get; set; } = new StringBuilder();
        private static Process CurrentProcess { get; set; }


        private string _LocalIp;
        public string LocalIp {
            get { return _LocalIp; }
            set {
                _LocalIp = value;
                RaisePropertyChanged();
            }
        }

        private bool _Starting;
        public bool Starting {
            get { return _Starting; }
            set {
                _Starting = value;
                RaisePropertyChanged();
                if (Starting)
                    CurrentStatus = ServerStatus.Starting;
            }
        }
        private ServerStatus _CurrentStatus;
        public ServerStatus CurrentStatus {
            get { return _CurrentStatus; }
            set {
                _CurrentStatus = value;
                RaisePropertyChanged();
                OnServerStatusChanged?.Invoke(value);
            }
        }
        private bool _DownLoading;
        public bool DownLoading {
            get { return _DownLoading; }
            set {
                _DownLoading = value;
                RaisePropertyChanged();
            }
        }
        private int _Progress;
        public int Progress {
            get { return _Progress; }
            set {
                _Progress = value;
                RaisePropertyChanged();
            }
        }


        #endregion


        public Window_Server()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public void Init()
        {
            // 获取本地 IP
            try {
                LocalIp = NetUtils.GetLocalIPAddress();
            } catch (Exception ex) {
                MessageCard.Error(ex.Message);
            }

            Logger.Info($"get local ip: {LocalIp}");
        }


        private async void BaseWindow_ContentRendered(object sender, System.EventArgs e)
        {
            Init();
            CurrentStatus = await ServerManager.CheckStatus();
            TextEditorOptions textEditorOptions = new TextEditorOptions();
            textEditorOptions.HighlightCurrentLine = true;
            logTextBox.Options = textEditorOptions;

            // 设置语法高亮
            logTextBox.SyntaxHighlighting = HighlightingManager.Instance.HighlightingDefinitions[0];

            if (LogCache.Length > 0)
                logTextBox.Text = LogCache.ToString();

        }




        private async void StartServer()
        {
            ClearLog();
            Starting = true;
            LogMsg("检查文件...");
            // 1. 检查文件是否存在
            if (!File.Exists(ServerManager.ServerFilePath) && !await ServerManager.DownloadJar()) {
                MessageCard.Error($"下载文件失败，请前往手动下载：{UrlManager.ServerUrl}");
                Starting = false;
                return;
            }

            LogMsg("文件最新");
            LogMsg("开始启动服务");

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // 需要去掉目录末尾的 /，否则 java 运行不对
            if (baseDir.EndsWith("\\") || baseDir.EndsWith("/"))
                baseDir = baseDir.Substring(0, baseDir.Length - 2);

            // 2. 启动服务端
            string cmdParams = $"-Xmx2048m -Dfile.encoding=UTF-8" +
                $" -Dloader.path=\"{ServerManager.ServerLibPath}\"" +
                $" -jar" +
                $" -Dserver.port={ConfigManager.JavaServerConfig.Port}" +
                $" -DSQLITE_DATA_FILE_NAME=\"{SqlManager.DEFAULT_SQLITE_PATH}\"" +
                $" -DSQLITE_DATA_PATH=\"{PathManager.CurrentUserFolder}\"" +
                $" -DSQLITE_CONFIG_PATH=\"{SqlManager.DEFAULT_SQLITE_CONFIG_PATH}\"" +
                $" -DEXE_PATH=\"{baseDir}\"" +
                $" \"{ServerManager.ServerFilePath}\"";


            Logger.Info($"run server with arg: java {cmdParams}");
            LogMsg("java " + cmdParams);

            bool success = true;
            await Task.Run(() => {
                CmdHelper.Run("java", cmdParams, (msg) => {
                    Log(msg);
                    if (msg.IndexOf("was already in use") >= 0) {
                        success = false;
                        return;
                    } else if (msg.IndexOf("jvedio server start ok!") >= 0) {
                        Starting = false;
                        CurrentStatus = ServerStatus.Ready;
                    }
                }, (err) => {
                    Log(err);
                }, (ex) => {
                    Log(ex.Message);
                    MessageCard.Error(ex.Message);
                }, onCreated: (p) => {
                    CurrentProcess = p;
                });
            });
            if (!success) {
                if ((bool)new MsgBox($"端口 {ConfigManager.JavaServerConfig.Port} 被占用，是否关闭对应进程并重启？").ShowDialog(this)) {
                    success = ProcessManager.KillByPort((int)ConfigManager.JavaServerConfig.Port);
                    if (success) {
                        LogMsg("关闭进程成功！");
                        await Task.Delay(1000);
                        StartServer();
                    } else {
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

            LogMsg("停止当前服务");
            CurrentProcess?.Close();
            bool success = ProcessManager.KillByPort((int)ConfigManager.JavaServerConfig.Port);
            if (success) {
                LogMsg("关闭进程成功");
                CurrentStatus = ServerStatus.UnReady;
            } else
                LogMsg("关闭进程失败");

        }

        private void StopServer(object sender, System.Windows.RoutedEventArgs e)
        {
            StopServer();
        }

        private void LogMsg(string msg)
        {
            string str = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [INFO ] => " + msg;
            Log(str);
        }

        private void Log(string msg)
        {
            App.Current.Dispatcher.Invoke(() => {
                logTextBox.AppendText(msg + Environment.NewLine);
                Logger.Info(msg);
                logTextBox.ScrollToEnd();
            });
            Console.WriteLine(msg);
            LogCache.AppendLine(msg);
        }
        private void ClearLog()
        {
            LogCache.Clear();
            App.Current.Dispatcher.Invoke(() => {
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

        private void textBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            Jvedio.AvalonEdit.Utils.GotFocus(sender);
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Jvedio.AvalonEdit.Utils.LostFocus(sender);
        }
    }
}
