using Jvedio.Core.Scan;
using SuperControls.Style;
using SuperUtils.IO;
using SuperUtils.Reflections;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Jvedio.App;


namespace Jvedio
{
    /// <summary>
    /// Window_ScanDetail.xaml 的交互逻辑
    /// </summary>
    public partial class Window_ScanDetail : BaseWindow
    {

        #region "属性"
        private ScanResult ScanResult { get; set; }
        private List<ScanDetail> Details { get; set; }

        #endregion

        public Window_ScanDetail()
        {
            InitializeComponent();
        }

        public Window_ScanDetail(ScanResult scanResult) : this()
        {
            ScanResult = scanResult;
        }


        private class ScanDetail
        {
            public long ID { get; set; }
            public string Handle { get; set; }

            public string Extension { get; set; }

            public string Reason { get; set; }
            public string Details { get; set; }

            public bool ShowDetail { get; set; }

            public string FilePath { get; set; }



            public override string ToString()
            {
                return ClassUtils.ToString(this);
            }
        }



        private string getExtension(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            return System.IO.Path.GetExtension(path).ToLower().Replace(".", string.Empty);
        }


        private void BaseWindow_ContentRendered(object sender, EventArgs e)
        {
            Details = new List<ScanDetail>();
            long idx = 0;
            foreach (var item in ScanResult.FailNFO) {
                ScanDetail detail = new ScanDetail() {
                    Handle = LangManager.GetValueByKey("NotImport"),
                    FilePath = item,
                    Extension = getExtension(item),
                    Reason = string.Empty,
                    ID = idx++,
                };
                Details.Add(detail);
            }

            foreach (var key in ScanResult.NotImport.Keys) {
                ScanDetail detail = new ScanDetail() {
                    Handle = LangManager.GetValueByKey("NotImport"),
                    FilePath = key,
                    Extension = getExtension(key),
                    Reason = ScanResult.NotImport[key].Reason,
                    Details = ScanResult.NotImport[key].Detail,
                    ShowDetail = !string.IsNullOrEmpty(ScanResult.NotImport[key].Detail),
                    ID = idx++,
                };
                Details.Add(detail);
            }

            foreach (var key in ScanResult.Update.Keys) {
                ScanDetail detail = new ScanDetail() {
                    Handle = LangManager.GetValueByKey("Update"),
                    FilePath = key,
                    Extension = getExtension(key),
                    Reason = ScanResult.Update[key],
                    ID = idx++,
                };
                Details.Add(detail);
            }

            foreach (var item in ScanResult.Import) {
                ScanDetail detail = new ScanDetail() {
                    Handle = LangManager.GetValueByKey("Import"),
                    FilePath = item,
                    Extension = getExtension(item),
                    Reason = string.Empty,
                    ID = idx++,
                };
                Details.Add(detail);
            }

            dataGrid.ItemsSource = Details;

            total.Text = ScanResult.TotalCount.ToString();
            import.Text = ScanResult.Import.Count.ToString();
            notImport.Text = ScanResult.NotImport.Count.ToString();
            update.Text = ScanResult.Update.Count.ToString();
            failNfo.Text = ScanResult.FailNFO.Count.ToString();
            scanDate.Text = ScanResult.ScanDate.ToString();
            cost.Text = DateHelper.ToReadableTime(ScanResult.ElapsedMilliseconds);
        }

        private void CopyPath(object sender, RoutedEventArgs e)
        {
            ScanDetail detail = dataGrid.SelectedItem as ScanDetail;
            if (ClipBoard.TrySetDataObject(detail.FilePath))
                MessageNotify.Success(SuperControls.Style.LangManager.GetValueByKey("HasCopy"));
        }

        private void OpenPath(object sender, RoutedEventArgs e)
        {
            ScanDetail detail = dataGrid.SelectedItem as ScanDetail;
            FileHelper.TryOpenSelectPath(detail.FilePath);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();

            saveFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    string path = saveFileDialog.FileName;
                    if (!path.ToLower().EndsWith(".csv"))
                        path += ".csv";
                    File.WriteAllText(path, GenerateOutput());
                    MessageNotify.Success(SuperControls.Style.LangManager.GetValueByKey("Message_Success"));
                    FileHelper.TryOpenSelectPath(path);
                } catch (Exception ex) {
                    Logger.Error(ex);
                    MessageCard.Error(ex.Message);
                }
            }
        }

        private string GenerateOutput()
        {
            List<ScanDetail> datas = new List<ScanDetail>();
            for (int i = 0; i < dataGrid.Items.Count; i++) {
                ScanDetail detail = (ScanDetail)dataGrid.Items[i];
                datas.Add(detail);
            }

            return ClassUtils.ToCsv(datas);
        }

        private void ShowExceptions(object sender, RoutedEventArgs e)
        {
            new Dialog_Logs(string.Join(Environment.NewLine, ScanResult.Logs)).ShowDialog(this);
        }

        private void ShowDetail(object sender, RoutedEventArgs e)
        {
            if (Details != null && Details.Count > 0) {
                if (sender is Button button && button.Tag != null &&
                    long.TryParse(button.Tag.ToString(), out long id)) {
                    ScanDetail scanDetail = Details.FirstOrDefault(arg => arg.ID == id);
                    if (scanDetail != null)
                        new Dialog_Logs(string.Join(Environment.NewLine, scanDetail.Details)).ShowDialog(this);
                }
            }
        }
    }
}
