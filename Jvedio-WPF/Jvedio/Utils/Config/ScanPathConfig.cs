using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using static Jvedio.GlobalVariable;

namespace Jvedio
{

    /// <summary>
    /// 数据库扫描路径
    /// </summary>
    public class ScanPathConfig
    {

        private string DataBase = "";
        private string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase");
        private string  filepath="" ;

        public ScanPathConfig(string databasename)
        {
            DataBase = databasename;
            if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);
            filepath = Path.Combine(baseDir, "ScanPathConfig");
        }

        public void Save(List<string> Paths)
        {
            
            InitXML();
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(filepath);
            XmlNode pathNodes = XmlDoc.SelectSingleNode($"/ScanPaths/DataBase[@Name='{DataBase}']");
            XmlNodeList xmlNodeList= XmlDoc.SelectNodes($"/ScanPaths/DataBase[@Name='{DataBase}']/Path");
            if(xmlNodeList!=null && xmlNodeList.Count > 0)
            {
                foreach(XmlNode item in xmlNodeList)
                {
                    pathNodes.RemoveChild(item);
                }
            }

            foreach (string path in Paths)
            {
                XmlElement xe = XmlDoc.CreateElement("Path");
                xe.InnerText = path;
                pathNodes.AppendChild(xe);
            }
            XmlDoc.Save(filepath);
        }

        public bool InitXML()
        {
            try
            {
                if (string.IsNullOrEmpty(DataBase)) return false;
            XmlDocument XmlDoc = new XmlDocument();
            string Root = "ScanPaths";
            bool CreateRoot = false;
            if (File.Exists(filepath))
            {
                try
                {
                    XmlDoc.Load(filepath);
                }
                catch { CreateRoot = true; }

            }
            else
            {
                CreateRoot = true;
            }


            if (CreateRoot)
            {

                try
                {
                    XmlNode header = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    XmlDoc.AppendChild(header);
                }
                catch { }

                //生成根节点
                var xm = XmlDoc.CreateElement(Root);
                XmlDoc.AppendChild(xm);
            }
            XmlElement rootElement = XmlDoc.DocumentElement;
            XmlNode node = XmlDoc.SelectSingleNode($"/ScanPaths/DataBase[@Name='{DataBase}']");
            if (node == null)
            {
                //不存在该节点
                XmlElement XE = XmlDoc.CreateElement("DataBase");
                XE.SetAttribute("Name", DataBase);
                rootElement.AppendChild(XE);
            }
            XmlDoc.Save(filepath);
            return true;
            }
            catch
            {
                return false;
            }
        }


        public StringCollection Read()
        {
            StringCollection result = new StringCollection();
            if (!File.Exists(filepath)) InitXML();
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(filepath);


            XmlNodeList pathNodes = XmlDoc.SelectNodes($"/ScanPaths/DataBase[@Name='{DataBase}']/Path");
            if(pathNodes!=null && pathNodes.Count > 0)
            {
                foreach(XmlNode xmlNode in pathNodes)
                {
                    if (!result.Contains(xmlNode.InnerText)) result.Add(xmlNode.InnerText);
                }
            }
            return result;
        }

    }

}
