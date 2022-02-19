using Jvedio.Utils.Common;
using System;
using System.Linq;
using System.Reflection;

namespace Jvedio.Core
{
    public class Settings
    {
        static Settings()
        {
            initTempClass();
        }

        static void initTempClass()
        {

        }

        private class tempClass
        {

        }
        public static class Common
        {
            public static Boolean OpenDataBaseDefault = true;
            public static class InnerTest
            {
                public static Boolean Test = true;
            }
        }

        public static class Scan
        {

        }

        public static class Download
        {

        }

        public static class View
        {

        }

        public static class Theme
        {
        }


        public static class Transaltion
        {

        }


        public static class AI
        {

        }

        public static class Video
        {

        }

        public static class CustomView
        {

        }

        public static class Rename
        {

        }


        public static string toJson()
        {
            return "{" + JsonUtils.ClasstoJson(typeof(Settings).GetTypeInfo().DeclaredNestedTypes.ToArray()) + "}";
        }

        public static void Load(object json)
        {
            Console.WriteLine(json);
        }











        public static void Save()
        {

        }

    }

}
