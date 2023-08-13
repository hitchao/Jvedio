
using Jvedio.Entity;
using SuperControls.Style;
using SuperControls.Style.Windows;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Wrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio
{
    /// <summary>
    /// Window_DataBase.xaml 的交互逻辑
    /// </summary>
    public partial class Window_DataBase : BaseWindow
    {
        #region "事件"
        public Action OnDataChanged;
        #endregion

        #region "属性"


        private Main Main { get; set; }

        private bool _Running = false;

        public bool Running {
            get { return _Running; }

            set {
                _Running = value;
                RaisePropertyChanged();
            }
        }
        private int _RunProgress;

        public int RunProgress {
            get { return _RunProgress; }

            set {
                _RunProgress = value;
                RaisePropertyChanged();
            }
        }


        #endregion

        public Window_DataBase()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            Log("初始化");
            this.DataContext = this;
            Main = SuperUtils.WPF.VisualTools.WindowHelper.GetWindowByName("Main", App.Current.Windows) as Main;
            Log("初始化完成");
        }

        private void Log(string text)
        {
            Logger.Debug(text);
            App.Current.Dispatcher.Invoke(() => {
                logTextBox.AppendText(text + Environment.NewLine);
                logTextBox.ScrollToEnd();
            });
        }

        public void ClearLog()
        {
            logTextBox.Clear();
        }


        private async void DeleteNotExistVideo(object sender, RoutedEventArgs e)
        {
            if (Main.IsTaskRunning()) {
                MessageNotify.Error(LangManager.GetValueByKey("NeedToClearTask"));
                return;
            }
            Log("开始执行任务：【删除库中不存在的影片】");

            Running = true;
            RunProgress = 0;

            await Task.Run(async () => {
                List<string> toDelete = new List<string>();
                SelectWrapper<MetaData> wrapper = new SelectWrapper<MetaData>();
                wrapper.Select("DataID", "Path").Eq("DBId", ConfigManager.Main.CurrentDBId).Eq("DataType", 0);
                List<MetaData> metaDatas = metaDataMapper.SelectList(wrapper);
                if (metaDatas?.Count <= 0) {
                    Running = false;
                    return;
                }

                Log("开始检查文件是否存在");
                int totalCount = metaDatas.Count;
                for (int i = 0; i < totalCount; i++) {
                    MetaData data = metaDatas[i];
                    if (!File.Exists(data.Path))
                        toDelete.Add(data.DataID.ToString());

                    RunProgress = (int)((double)i / (double)totalCount * 100);
                }
                Log($"需要删除的数目：{toDelete.Count}");
                if (toDelete.Count <= 0) {
                    Dispatcher.Invoke(() => {
                        MessageNotify.Info(LangManager.GetValueByKey("AllDataExistsNoOperation"));
                    });
                    Running = false;
                    return;
                }
                bool confirm = false;
                await Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate {
                    confirm = (bool)new MsgBox($"{LangManager.GetValueByKey("IsToDeleteFromLibrary")} {toDelete.Count} {LangManager.GetValueByKey("VideoNotExists")}")
                       .ShowDialog(this);
                });

                if (confirm) {
                    try {
                        Log($"开始删除 {toDelete.Count} 个信息");
                        videoMapper.deleteVideoByIds(toDelete);
                        await Task.Delay(5000); // todo 删除不存在的信息
                        OnDataChanged?.Invoke();
                    } catch (Exception ex) {
                        Log(ex.Message);
                    }
                }
                Running = false;
                Log($"删除完成");
            });
        }
        private async void DeleteNotInScanPath(object sender, RoutedEventArgs e)
        {
            if (Main.IsTaskRunning()) {
                MessageNotify.Error(LangManager.GetValueByKey("NeedToClearTask"));
                return;
            }

            string scanPath = Main.vieModel.CurrentAppDataBase.ScanPath;
            if (string.IsNullOrEmpty(scanPath)) {
                MessageNotify.Error(LangManager.GetValueByKey("LibraryNotSetPath"));
                return;
            }

            List<string> scanPaths = JsonUtils.TryDeserializeObject<List<string>>(scanPath).Where(arg => !string.IsNullOrEmpty(arg)).ToList();
            if (scanPaths == null || scanPaths.Count <= 0) {
                MessageNotify.Error(LangManager.GetValueByKey("LibraryNotSetPath"));
                return;
            }

            Running = true;
            RunProgress = 0;
            Log("开始执行任务：【删除不位于启动时扫描】目录中的影片");
            await Task.Run(async () => {
                List<string> toDelete = new List<string>();
                SelectWrapper<MetaData> wrapper = new SelectWrapper<MetaData>();
                wrapper.Select("DataID", "Path").Eq("DBId", ConfigManager.Main.CurrentDBId).Eq("DataType", 0);
                List<MetaData> metaDatas = metaDataMapper.SelectList(wrapper);
                if (metaDatas?.Count <= 0) {
                    Running = false;
                    return;
                }

                int totalCount = metaDatas.Count;
                for (int i = 0; i < totalCount; i++) {
                    MetaData data = metaDatas[i];
                    string path = data.Path;
                    if (string.IsNullOrEmpty(path)) {
                        toDelete.Add(data.DataID.ToString());
                        continue;
                    }

                    foreach (string dir in scanPaths) {
                        if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(dir))
                            continue;
                        if (path.IndexOf(dir) < 0) {
                            toDelete.Add(data.DataID.ToString());
                            break;
                        }
                    }
                    RunProgress = (int)((double)i / (double)totalCount * 100);
                }
                Log($"需要删除的数目：{toDelete.Count}");
                if (toDelete.Count <= 0) {
                    Dispatcher.Invoke(() => {
                        MessageNotify.Info(LangManager.GetValueByKey("AllDataExistsNoOperation"));
                    });
                    Running = false;
                    return;
                }
                bool confirm = false;
                await Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate {
                    confirm = (bool)new MsgBox($"{LangManager.GetValueByKey("IsToDeleteFromLibrary")} {toDelete.Count} {LangManager.GetValueByKey("VideoNotInScanStatupDir")}")
                         .ShowDialog(this);
                });

                if (confirm) {
                    try {
                        Log($"开始删除 {toDelete.Count} 个信息");
                        videoMapper.deleteVideoByIds(toDelete);
                        await Task.Delay(5000); // todo 删除不位于库关联目录
                        OnDataChanged?.Invoke();
                    } catch (Exception ex) {
                        Log(ex.Message);
                    }
                }

                Running = false;
                Log($"删除完成");
            });
        }

        private void ExportToNFO(object sender, RoutedEventArgs e)
        {
            if (Main.IsTaskRunning()) {
                MessageNotify.Error(LangManager.GetValueByKey("NeedToClearTask"));
                return;
            }
        }
    }
}
