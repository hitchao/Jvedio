using System;
using System.Collections.Generic;
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
    /// 保存服务器配置到XML文件
    /// </summary>
    public sealed  class ServerConfig
    {
        private string Root = "Servers";
        private bool CreateRoot = false;
        private string ConfigPath = "ServersConfig";
        private static string[] Nodes = new[] { "Url", "IsEnable", "LastRefreshDate", "Cookie" };
        private XmlDocument XmlDoc;

        private static readonly ServerConfig instance = new ServerConfig();


        private ServerConfig()
        {
            InitXML();
        }

        public static ServerConfig Instance
        {
            get
            {
                return instance;
            }
        }




        public void InitXML()
        {
            XmlDoc = new XmlDocument();
            if (File.Exists(ConfigPath))
            {
                //是否是标准的 xml 格式
                try
                {
                    XmlDoc.Load(ConfigPath);
                }
                catch (Exception ex)
                {
                    Logger.LogF(ex);
                    CreateRoot = true;
                }
            }
            else
            {
                CreateRoot = true;
            }


            //根节点
            if (CreateRoot)
            {
                XmlNode header = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                XmlDoc.AppendChild(header);
                var xm = XmlDoc.CreateElement(Root);
                XmlDoc.AppendChild(xm);
            }


            //子节点
            foreach (var item in typeof(Servers).GetProperties())
            {
                XmlElement rootElement = XmlDoc.DocumentElement;
                XmlNode node = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{item.Name}']");
                if (node == null)
                {
                    //不存在该节点
                    XmlElement XE = XmlDoc.CreateElement("Server");
                    XE.SetAttribute("Name", item.Name);
                    rootElement.AppendChild(XE);
                }
            }

            foreach (var item in typeof(Servers).GetProperties())
            {
                XmlElement rootElement = XmlDoc.DocumentElement;
                XmlNode node = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{item.Name}']");
                //子子节点
                foreach (var Node in Nodes)
                {
                    XmlNode xn = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{item.Name}']/{Node}");
                    if (xn == null)
                    {
                        XmlElement xe = XmlDoc.CreateElement(Node);
                        xe.InnerText = "";
                        node.AppendChild(xe);
                    }
                }
            }
            try
            {
                XmlDoc.Save(ConfigPath);
            }catch(XmlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SaveServer(Server server)
        {
            if (server.Name == "") return;
            if (server.Cookie == Jvedio.Language.Resources.Nothing) server.Cookie = "";
            Type type = server.GetType();
            foreach (var Node in Nodes)
            {
                XmlNode xn = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{server.Name}']/{Node}");
                if (xn != null)
                {
                    System.Reflection.PropertyInfo propertyInfo = type.GetProperty(Node);
                    xn.InnerText = propertyInfo.GetValue(server).ToString();
                }
            }
            Save();
        }




        private void Save()
        {
            XmlDoc.Save(ConfigPath);
        }



        public string ReadByName(string name,string node)
        {
            XmlNode x1 = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{name}']/{node}");
            if (x1 != null) return x1.InnerText;
            return "";
        }


        public Servers ReadAll()
        {
            Servers result = new Servers();
            Type type = result.GetType();
            foreach (var item in type.GetProperties())
            {
                ServerConfig serverConfig = new ServerConfig();
                Server server = new Server(item.Name)
                {
                    Cookie = serverConfig.ReadByName(item.Name,"Cookie"),
                    LastRefreshDate = serverConfig.ReadByName(item.Name, "LastRefreshDate"),
                    Url = serverConfig.ReadByName(item.Name, "Url"),

                };
                bool.TryParse(serverConfig.ReadByName(item.Name, "IsEnable"), out bool enable);
                server.IsEnable = enable;
                if (server.Cookie == Jvedio.Language.Resources.Nothing) server.Cookie = "";
                System.Reflection.PropertyInfo propertyInfo = type.GetProperty(item.Name);
                propertyInfo.SetValue(result, server, null);
            }
            return result;
        }


        public void  DeleteByName(string name)
        {
            XmlNode x1 = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{name}']");
            XmlElement root = XmlDoc.DocumentElement;
            if (x1 != null) {
                root.RemoveChild(x1);
                Save();
            }
        }

    }









}
