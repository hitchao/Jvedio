using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Jvedio
{
    /// <summary>
    /// 使用XML 文件存储最近观看的影片
    /// </summary>
    ///
    [Obsolete]
    public class RecentWatchedConfig
    {
        private string Date = string.Empty;
        private string filepath = "RecentWatch";

        public RecentWatchedConfig(string date = "")
        {
            Date = date;
        }

        public bool InitXML()
        {
            try {
                if (string.IsNullOrEmpty(Date))
                    return false;
                XmlDocument xmlDoc = new XmlDocument();
                string Root = "RecentWatch";
                bool CreateRoot = false;
                if (File.Exists(filepath)) {
                    try {
                        xmlDoc.Load(filepath);
                    } catch {
                        CreateRoot = true;
                    }
                } else {
                    CreateRoot = true;
                }

                if (CreateRoot) {
                    try {
                        XmlNode header = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                        xmlDoc.AppendChild(header);
                    } catch {
                    }

                    // 生成根节点
                    var xm = xmlDoc.CreateElement(Root);
                    xmlDoc.AppendChild(xm);
                }

                XmlElement rootElement = xmlDoc.DocumentElement;
                XmlNode node = xmlDoc.SelectSingleNode($"/RecentWatch/Date[@Name='{Date}']");
                if (node == null) {
                    // 不存在该节点
                    XmlElement XE = xmlDoc.CreateElement("Date");
                    XE.SetAttribute("Name", Date);
                    rootElement.AppendChild(XE);
                }

                xmlDoc.Save(filepath);
                return true;
            } catch {
                return false;
            }
        }

        public void Save(List<string> iDs)
        {
            InitXML();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filepath);
            XmlNode pathNodes = xmlDoc.SelectSingleNode($"/RecentWatch/Date[@Name='{Date}']");
            XmlNodeList xmlNodeList = xmlDoc.SelectNodes($"/RecentWatch/Date[@Name='{Date}']/ID");
            if (xmlNodeList != null && xmlNodeList.Count > 0) {
                foreach (XmlNode item in xmlNodeList) {
                    pathNodes.RemoveChild(item);
                }
            }

            foreach (string path in iDs) {
                XmlElement xe = xmlDoc.CreateElement("ID");
                xe.InnerText = path;
                pathNodes.AppendChild(xe);
            }

            xmlDoc.Save(filepath);
        }

        public bool Clear()
        {
            if (!File.Exists(filepath))
                InitXML();
            XmlDocument xmlDoc = new XmlDocument();
            try {
                xmlDoc.Load(filepath);
                XmlElement root = xmlDoc.DocumentElement;
                root.RemoveAll();
                xmlDoc.Save(filepath);
            } catch {
                return false;
            }

            return true;
        }

        public Dictionary<DateTime, List<string>> Read()
        {
            Dictionary<DateTime, List<string>> result = new Dictionary<DateTime, List<string>>();
            if (!File.Exists(filepath))
                InitXML();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filepath);
            XmlNodeList dateNodes = xmlDoc.SelectNodes($"/RecentWatch/Date");

            if (dateNodes != null && dateNodes.Count > 0) {
                foreach (XmlNode xmlNode in dateNodes) {
                    string date = xmlNode.Attributes[0].Value;
                    if (!string.IsNullOrEmpty(date)) {
                        XmlNodeList IDNodes = xmlDoc.SelectNodes($"/RecentWatch/Date[@Name='{date}']/ID");
                        if (IDNodes != null && IDNodes.Count > 0) {
                            DateTime dateTime;
                            bool success = DateTime.TryParse(date, out dateTime);
                            List<string> id = new List<string>();
                            foreach (XmlNode item in IDNodes) {
                                if (!id.Contains(item.InnerText))
                                    id.Add(item.InnerText);
                            }

                            if (success) {
                                if (!result.ContainsKey(dateTime))
                                    result.Add(dateTime, id);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public void Remove(DateTime dateTime)
        {
            InitXML();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filepath);

            XmlNode root = xmlDoc.SelectSingleNode($"/RecentWatch");
            XmlNode node = xmlDoc.SelectSingleNode($"/RecentWatch/Date[@Name='{dateTime.ToString("yyyy-MM-dd")}']");
            if (root != null && node != null) {
                root.RemoveChild(node);
            }

            xmlDoc.Save(filepath);
        }
    }
}
