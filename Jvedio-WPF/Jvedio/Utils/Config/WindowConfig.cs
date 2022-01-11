using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using static Jvedio.GlobalVariable;
using Jvedio.Utils;

namespace Jvedio
{
    /// <summary>
    /// 保存窗体的启动位置、状态
    /// </summary>
    public class WindowConfig
    {
        private string WindowName = "";

        public WindowConfig(string windowname)
        {
            WindowName = windowname;
        }

        public void Save(WindowProperty  windowProperty)
        {
            string filepath ="WindowConfig";
            InitXML();
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(filepath);
            XmlNode x1 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/X");
            XmlNode x2 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/Y");
            XmlNode x3 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/Width");
            XmlNode x4 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/Height");
            XmlNode x5 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/winstate");
            if (x1 != null) x1.InnerText = windowProperty.Location.X.ToString();
            if (x2 != null) x2.InnerText = windowProperty.Location.Y.ToString();
            if (x3 != null) x3.InnerText = windowProperty.Size.Width.ToString();
            if (x4 != null) x4.InnerText = windowProperty.Size.Height.ToString();
            if (x5 != null) x5.InnerText = windowProperty.WinState.ToString();
            XmlDoc.Save(filepath);
        }

        public bool InitXML()
        {
            try
            {
                if (string.IsNullOrEmpty(WindowName)) return false;
            XmlDocument XmlDoc = new XmlDocument();
            string Root = "Windows";
            bool CreateRoot = false;
            if (File.Exists("WindowConfig"))
            {
                try
                {
                    XmlDoc.Load("WindowConfig");
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
            XmlNode node = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']");
            if (node == null)
            {
                //不存在该节点
                XmlElement XE = XmlDoc.CreateElement("Window");
                XE.SetAttribute("Name", WindowName);
                XmlElement x1 = XmlDoc.CreateElement("X");
                x1.InnerText = "200";
                XmlElement x2 = XmlDoc.CreateElement("Y");
                x2.InnerText = "200";
                XmlElement x3 = XmlDoc.CreateElement("Width");
                x3.InnerText = "500";
                XmlElement x4 = XmlDoc.CreateElement("Height");
                x4.InnerText = "500";
                XmlElement x5 = XmlDoc.CreateElement("winstate");
                x5.InnerText = "Normal";

                XE.AppendChild(x1);
                XE.AppendChild(x2);
                XE.AppendChild(x3);
                XE.AppendChild(x4);
                XE.AppendChild(x5);
                rootElement.AppendChild(XE);
            }
            else
            {
                XmlNode x1 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/X");
                XmlNode x2 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/Y");
                XmlNode x3 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/Width");
                XmlNode x4 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/Height");
                XmlNode x5 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/winstate");
                if (x1 == null)
                {
                    XmlElement xe1 = XmlDoc.CreateElement("X");
                    xe1.InnerText = "200";
                    node.AppendChild(xe1);
                }

                if (x2 == null)
                {
                    XmlElement xe2 = XmlDoc.CreateElement("Y");
                    xe2.InnerText = "200";
                    node.AppendChild(xe2);
                }

                if (x3 == null)
                {
                    XmlElement xe3 = XmlDoc.CreateElement("Width");
                    xe3.InnerText = "500";
                    node.AppendChild(xe3);
                }

                if (x4 == null)
                {
                    XmlElement xe4 = XmlDoc.CreateElement("Height");
                    xe4.InnerText = "500";
                    node.AppendChild(xe4);
                }

                if (x5 == null)
                {
                    XmlElement xe5 = XmlDoc.CreateElement("winstate");
                    xe5.InnerText = "Normal";
                    node.AppendChild(xe5);
                }

            }
            XmlDoc.Save("WindowConfig");
            return true;
            }
            catch
            {
                return false;
            }
        }


        public WindowProperty Read()
        {
            WindowProperty windowProperty = new WindowProperty();
            string filepath = "WindowConfig";
            if (!File.Exists(filepath)) InitXML();
            XmlDocument XmlDoc = new XmlDocument();
            
            try
            {
                XmlDoc.Load(filepath);
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return windowProperty;//缺少根元素
            }
            
            XmlNode x1 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/X");
            XmlNode x2 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/Y");
            XmlNode x3 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/Width");
            XmlNode x4 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/Height");
            XmlNode x5 = XmlDoc.SelectSingleNode($"/Windows/Window[@Name='{WindowName}']/winstate");



            double x=0,y=0,w=0, h = 0;
            double.TryParse(x1?.InnerText, out x);
            double.TryParse(x2?.InnerText, out y);
            double.TryParse(x3?.InnerText, out w);
            double.TryParse(x4?.InnerText, out h);

            JvedioWindowState jvedioWindowState = JvedioWindowState.Normal;
            Enum.TryParse<JvedioWindowState>(x5?.InnerText, out jvedioWindowState);

            windowProperty.Location = new Point(x,y);
            windowProperty.Size = new Size(w, h);
            windowProperty.WinState = jvedioWindowState;
            return windowProperty;
        }

    }

}
