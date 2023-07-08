using Jvedio.Core.Global;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;

namespace Jvedio
{
    /// <summary>
    /// 数据库扫描路径
    /// </summary>
    [Obsolete]
    public class ScanPathConfig
    {
        private string DataBase = "info";
        private string baseDir = PathManager.CurrentUserFolder;
        private string filepath = Path.Combine(PathManager.oldDataPath, "ScanPathConfig");

        public ScanPathConfig(string databaseName)
        {
            if (!string.IsNullOrEmpty(databaseName))
                DataBase = databaseName;
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);
        }

        public void Save(List<string> paths)
        {
            InitXML();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filepath);
            XmlNode pathNodes = xmlDoc.SelectSingleNode($"/ScanPaths/DataBase[@Name='{DataBase}']");
            XmlNodeList xmlNodeList = xmlDoc.SelectNodes($"/ScanPaths/DataBase[@Name='{DataBase}']/Path");
            if (xmlNodeList != null && xmlNodeList.Count > 0) {
                foreach (XmlNode item in xmlNodeList) {
                    pathNodes.RemoveChild(item);
                }
            }

            foreach (string path in paths) {
                XmlElement xe = xmlDoc.CreateElement("Path");
                xe.InnerText = path;
                pathNodes.AppendChild(xe);
            }

            xmlDoc.Save(filepath);
        }

        public bool InitXML()
        {
            try {
                if (string.IsNullOrEmpty(DataBase))
                    return false;
                XmlDocument xmlDoc = new XmlDocument();
                string Root = "ScanPaths";
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
                XmlNode node = xmlDoc.SelectSingleNode($"/ScanPaths/DataBase[@Name='{DataBase}']");
                if (node == null) {
                    // 不存在该节点
                    XmlElement XE = xmlDoc.CreateElement("DataBase");
                    XE.SetAttribute("Name", DataBase);
                    rootElement.AppendChild(XE);
                }

                xmlDoc.Save(filepath);
                return true;
            } catch {
                return false;
            }
        }

        public StringCollection Read()
        {
            StringCollection result = new StringCollection();
            if (!File.Exists(filepath))
                InitXML();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filepath);

            XmlNodeList pathNodes = xmlDoc.SelectNodes($"/ScanPaths/DataBase[@Name='{DataBase}']/Path");
            if (pathNodes != null && pathNodes.Count > 0) {
                foreach (XmlNode xmlNode in pathNodes) {
                    if (!result.Contains(xmlNode.InnerText))
                        result.Add(xmlNode.InnerText);
                }
            }

            return result;
        }
    }
}
