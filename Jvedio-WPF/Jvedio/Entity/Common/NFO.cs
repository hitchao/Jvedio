using System;
using System.IO;
using System.Xml;
using static Jvedio.LogManager;

namespace Jvedio.Entity
{
    /// <summary>
    /// NFO
    /// </summary>
    public class NFO
    {
        // NFO 标准：https://kodi.wiki/view/NFO_files/Movies
        private XmlDocument XmlDoc = new XmlDocument();
        private string FilePath = string.Empty;

        public NFO(string fP, string rootNodeName)
        {
            FilePath = fP;
            try {
                XmlNode header = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                XmlDoc.AppendChild(header);
                var xm = XmlDoc.CreateElement(rootNodeName);
                XmlDoc.AppendChild(xm);
                CreateNewNode("source");
                CreateNewNode("plot");
                CreateNewNode("title");
                CreateNewNode("director");
                CreateNewNode("rating");
                CreateNewNode("criticrating");
                CreateNewNode("year");
                CreateNewNode("mpaa");
                CreateNewNode("customrating");
                CreateNewNode("countrycode");
                CreateNewNode("premiered");
                CreateNewNode("release");
                CreateNewNode("runtime");
                CreateNewNode("country");
                CreateNewNode("studio");
                CreateNewNode("id");
                CreateNewNode("num");
                XmlDoc.Save(fP);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void CreateNewNode(string nodeName, string nodeText = "", string nodeID = "", string nodeIDValue = "")
        {
            var root = XmlDoc.DocumentElement;
            XmlElement xE = XmlDoc.CreateElement(nodeName);
            if (!string.IsNullOrEmpty(nodeID))
                xE.SetAttribute(nodeID, nodeIDValue);
            xE.InnerText = nodeText;
            root.AppendChild(xE);
        }

        public void AppendNewNode(string nodeName, string nodeText = "", string nodeID = "", string nodeIDValue = "")
        {
            try {
                XmlDoc.Load(FilePath);
                var root = XmlDoc.DocumentElement;
                XmlElement xE = null;
                xE = XmlDoc.CreateElement(nodeName);
                if (!string.IsNullOrEmpty(nodeID))
                    xE.SetAttribute(nodeID, nodeIDValue);
                xE.InnerText = nodeText;
                root.AppendChild(xE);
                XmlDoc.Save(FilePath);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public void AppendNodeToNode(string fatherNode, string nodeName, string nodeText = "", string nodeID = "", string nodeIDValue = "")
        {
            if (File.Exists(FilePath) && !string.IsNullOrEmpty(nodeName) && !string.IsNullOrEmpty(fatherNode)) {
                XmlDoc.Load(FilePath);
                var xE = XmlDoc.CreateElement(nodeName);
                if (!string.IsNullOrEmpty(nodeID))
                    xE.SetAttribute(nodeID, nodeIDValue);
                xE.InnerText = nodeText;
                var fatherList = XmlDoc.GetElementsByTagName(fatherNode);
                var father = fatherList[fatherList.Count - 1];
                father.AppendChild(xE);
                XmlDoc.Save(FilePath);
            }
        }

        public string ReadNodeFromXML(string nodeName)
        {
            if (File.Exists(FilePath) && !string.IsNullOrEmpty(nodeName)) {
                XmlDoc.Load(FilePath);
                var xN = XmlDoc.GetElementsByTagName(nodeName)[0];
                if (xN is object)
                    return xN.InnerText;
                else
                    return string.Empty;
            } else {
                return string.Empty;
            }
        }

        public void SetNodeText(string nodeName, string nodeText)
        {
            if (File.Exists(FilePath) && !string.IsNullOrEmpty(nodeName)) {
                XmlDoc.Load(FilePath);
                var xN = XmlDoc.GetElementsByTagName(nodeName)[0];
                if (xN is object) {
                    xN.InnerText = nodeText;
                    XmlDoc.Save(FilePath);
                }
            }
        }


    }

}