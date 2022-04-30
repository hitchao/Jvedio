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
    /// 使用XML 文件存储最近观看的影片
    /// </summary>
    public class RecentWatchedConfig
    {
        private string Date = "";
        private string  filepath= "RecentWatch";

        public RecentWatchedConfig(string date="")
        {
            Date = date;
        }

        public bool InitXML()
        {
            try
            {
                if (string.IsNullOrEmpty(Date)) return false;
                XmlDocument XmlDoc = new XmlDocument();
                string Root = "RecentWatch";
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
                XmlNode node = XmlDoc.SelectSingleNode($"/RecentWatch/Date[@Name='{Date}']");
                if (node == null)
                {
                    //不存在该节点
                    XmlElement XE = XmlDoc.CreateElement("Date");
                    XE.SetAttribute("Name", Date);
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

        public void Save(List<string> IDs)
        {
            InitXML();
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(filepath);
            XmlNode pathNodes = XmlDoc.SelectSingleNode($"/RecentWatch/Date[@Name='{Date}']");
            XmlNodeList xmlNodeList= XmlDoc.SelectNodes($"/RecentWatch/Date[@Name='{Date}']/ID");
            if(xmlNodeList!=null && xmlNodeList.Count > 0)
            {
                foreach(XmlNode item in xmlNodeList)
                {
                    pathNodes.RemoveChild(item);
                }
            }

            foreach (string path in IDs)
            {
                XmlElement xe = XmlDoc.CreateElement("ID");
                xe.InnerText = path;
                pathNodes.AppendChild(xe);
            }
            XmlDoc.Save(filepath);
        }


        public bool  Clear()
        {
            if (!File.Exists(filepath)) InitXML();
            XmlDocument XmlDoc = new XmlDocument();
            try { 
            XmlDoc.Load(filepath);
            XmlElement root = XmlDoc.DocumentElement;
            root.RemoveAll();
                XmlDoc.Save(filepath);
            }
            catch { return false; }
            return true;
        }

        public Dictionary<DateTime, List<string>> Read()
        {
            Dictionary<DateTime, List<string>> result = new Dictionary<DateTime, List<string>>();
            if (!File.Exists(filepath)) InitXML();
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(filepath);
            XmlNodeList dateNodes = XmlDoc.SelectNodes($"/RecentWatch/Date");
            
            if (dateNodes != null && dateNodes.Count > 0)
            {
                foreach(XmlNode xmlNode in dateNodes)
                {
                    string date = xmlNode.Attributes[0].Value;
                    if (!string.IsNullOrEmpty(date))
                    {
                        XmlNodeList IDNodes = XmlDoc.SelectNodes($"/RecentWatch/Date[@Name='{date}']/ID");
                        if(IDNodes!=null && IDNodes.Count > 0)
                        {
                            DateTime dateTime;
                            bool success = DateTime.TryParse(date, out dateTime);
                            List<string> id = new List<string>();
                            foreach (XmlNode item in IDNodes) { if (!id.Contains(item.InnerText)) id.Add(item.InnerText); }
                            if (success)
                            {
                                if (!result.ContainsKey(dateTime)) result.Add(dateTime, id);
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
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(filepath);

            XmlNode root = XmlDoc.SelectSingleNode($"/RecentWatch");
            XmlNode node = XmlDoc.SelectSingleNode($"/RecentWatch/Date[@Name='{dateTime.ToString("yyyy-MM-dd")}']");
            if (root != null && node!=null)
            {
                root.RemoveChild(node);
            }

            XmlDoc.Save(filepath);
        }





    }

}
