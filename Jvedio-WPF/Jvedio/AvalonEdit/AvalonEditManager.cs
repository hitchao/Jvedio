using ICSharpCode.AvalonEdit.Highlighting;
using SuperUtils.IO;
using System;
using System.IO;
using System.Xml;
using static Jvedio.App;

namespace Jvedio.AvalonEdit
{
    public static class AvalonEditManager
    {
        public static string HighLightPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                 "AvalonEdit", "Highlighting");

        public static void Init()
        {
            Logger.Info("init high light");
            HighlightingManager.Instance.Clear();
            string[] xshd_list = FileHelper.TryGetAllFiles(HighLightPath, "*.xshd");
            foreach (var xshdPath in xshd_list) {
                try {
                    Logger.Info($"load xshd file: {xshdPath}");
                    IHighlightingDefinition customHighlighting;
                    using (Stream s = File.OpenRead(xshdPath)) {
                        if (s == null)
                            throw new InvalidOperationException("Could not find embedded resource");
                        using (XmlReader reader = new XmlTextReader(s)) {

                            customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                                HighlightingLoader.Load(reader, HighlightingManager.Instance);
                        }
                    }
                    // 检查是否在数据库中存在
                    string name = customHighlighting.Name;
                    HighlightingManager.Instance.RegisterHighlighting(name, null, customHighlighting);
                } catch (Exception ex) {
                    Logger.Error(ex);
                    continue;
                }
            }
        }
    }
}
