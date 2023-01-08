using Jvedio.Core.Logs;
using Jvedio.Core.Scan;
using SuperControls.Style;
using SuperUtils.IO;
using SuperUtils.Reflections;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Jvedio
{
    /// <summary>
    /// Window_ScanDetail.xaml 的交互逻辑
    /// </summary>
    public partial class Window_ScanDetail : BaseWindow
    {
        public Window_ScanDetail()
        {
            InitializeComponent();
        }

        public ScanResult ScanResult { get; set; }

        public class ScanDetail
        {
            public string Handle { get; set; }

            public string Extension { get; set; }

            public string Reason { get; set; }

            public string FilePath { get; set; }

            public override string ToString()
            {
                return ClassUtils.ToString(this);
            }
        }

        public Window_ScanDetail(ScanResult scanResult) : this()
        {
            ScanResult = scanResult;
        }

        private string getExtension(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return System.IO.Path.GetExtension(path).ToLower().Replace(".", string.Empty);
        }

        private void BaseWindow_ContentRendered(object sender, EventArgs e)
        {
            List<ScanDetail> details = new List<ScanDetail>();

            foreach (var item in ScanResult.FailNFO)
            {
                ScanDetail detail = new ScanDetail()
                {
                    Handle = LangManager.GetValueByKey("NotImport"),
                    FilePath = item,
                    Extension = getExtension(item),
                    Reason = string.Empty,
                };
                details.Add(detail);
            }

            foreach (var key in ScanResult.NotImport.Keys)
            {
                ScanDetail detail = new ScanDetail()
                {
                    Handle = LangManager.GetValueByKey("NotImport"),
                    FilePath = key,
                    Extension = getExtension(key),
                    Reason = ScanResult.NotImport[key],
                };
                details.Add(detail);
            }

            foreach (var key in ScanResult.Update.Keys)
            {
                ScanDetail detail = new ScanDetail()
                {
                    Handle = LangManager.GetValueByKey("Update"),
                    FilePath = key,
                    Extension = getExtension(key),
                    Reason = ScanResult.Update[key],
                };
                details.Add(detail);
            }

            foreach (var item in ScanResult.Import)
            {
                ScanDetail detail = new ScanDetail()
                {
                    Handle = LangManager.GetValueByKey("Import"),
                    FilePath = item,
                    Extension = getExtension(item),
                    Reason = string.Empty,
                };
                details.Add(detail);
            }

            dataGrid.ItemsSource = details;

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
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string path = saveFileDialog.FileName;
                    if (!path.ToLower().EndsWith(".txt")) path += ".txt";
                    File.WriteAllText(path, GenerateOutput());
                    MessageNotify.Success(SuperControls.Style.LangManager.GetValueByKey("Message_Success"));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    MessageCard.Error(ex.Message);
                }
            }
        }

        private string GenerateOutput()
        {
            StringBuilder builder = new StringBuilder();
            List<Border> borders = wrapPanel.Children.OfType<Border>().ToList();
            for (int i = 0; i < borders.Count; i++)
            {
                Border border = borders[i];
                UIElementCollection children = (border.Child as Grid).Children;
                foreach (var item in children)
                {
                    if (item.GetType().GetProperty("Text") != null)
                    {
                        string text = item.GetType().GetProperty("Text").GetValue(item).ToString();
                        builder.Append(text + " ");
                    }
                }

                builder.Append(Environment.NewLine);
            }

            builder.Append(LangManager.GetValueByKey("Detail"));
            builder.Append(Environment.NewLine);
            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                ScanDetail detail = (ScanDetail)dataGrid.Items[i];
                builder.Append(detail.ToString());
                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }

        private void ShowExceptions(object sender, RoutedEventArgs e)
        {
            new Dialog_Logs(this, string.Join(Environment.NewLine, ScanResult.Logs)).ShowDialog();
        }
    }
}
