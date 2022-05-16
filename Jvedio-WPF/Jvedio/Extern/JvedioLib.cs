﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JvedioLib.Security
{

    /// <summary>
    /// 该类是加密混淆后的 dll 方法反射加载，避免 aes key 暴露
    /// </summary>
    public static class Encrypt
    {

        public static Type type { get; set; }

        static Encrypt()
        {
            string dllPath = "JvedioLib.dll";
            Assembly dll = Assembly.LoadFrom(dllPath);
            Type[] types = dll.GetTypes();
            type = types.Where(arg => arg.Name.Equals("Encrypt")).FirstOrDefault();

        }


        private static object InvokeMethod(object[] _params, [CallerMemberName] string callerName = "")
        {
            if (string.IsNullOrEmpty(callerName)) return null;
            MethodInfo methodInfo = type.GetMethod(callerName);
            object value = methodInfo.Invoke(null, _params);
            return value;
        }

        public static string AesEncrypt(string str, string key)
        {
            object result = InvokeMethod(new object[] { str, key });
            return result == null ? "" : result.ToString();
        }
        public static string AesDecrypt(string str, string key)
        {
            object result = InvokeMethod(new object[] { str, key });
            return result == null ? "" : result.ToString();
        }


        public static string AesEncrypt(string str, int key = 0)
        {
            object result = InvokeMethod(new object[] { str, key }, "AesDecryptByIndex");
            return result == null ? "" : result.ToString();
        }



        public static string AesDecrypt(string str, int key = 0)
        {
            object result = InvokeMethod(new object[] { str, key }, "AesDecryptByIndex");
            return result == null ? "" : result.ToString();
        }


        public static string CalculateMD5Hash(string input)
        {
            object result = InvokeMethod(new object[] { input });
            return result == null ? "" : result.ToString();
        }



        public static string GetFileMD5(string filename)
        {
            object result = InvokeMethod(new object[] { filename });
            return result == null ? "" : result.ToString();
        }


        public static string FasterMd5(string filePath)
        {
            object result = InvokeMethod(new object[] { filePath });
            return result == null ? "" : result.ToString();
        }



        public static string FasterDirMD5(List<string> filePathsInOneDir)
        {
            object result = InvokeMethod(new object[] { filePathsInOneDir });
            return result == null ? "" : result.ToString();
        }


        public static string GetFilesMD5(string[] files)
        {
            object result = InvokeMethod(new object[] { files });
            return result == null ? "" : result.ToString();
        }

        public static string GetDirectorySize(string folderPath)
        {
            object result = InvokeMethod(new object[] { folderPath });
            return result == null ? "" : result.ToString();
        }


        public static string GetDirectoryMD5(string folderPath)
        {
            object result = InvokeMethod(new object[] { folderPath });
            return result == null ? "" : result.ToString();
        }





    }

    public static class Identify
    {

        private static object InvokeMethod(object[] _params, [CallerMemberName] string callerName = "")
        {
            if (string.IsNullOrEmpty(callerName)) return null;
            MethodInfo methodInfo = type.GetMethod(callerName);
            object value = methodInfo.Invoke(null, _params);
            return value;
        }

        public static Type type { get; set; }

        static Identify()
        {
            string dllPath = "JvedioLib.dll";
            Assembly dll = Assembly.LoadFrom(dllPath);
            Type[] types = dll.GetTypes();
            type = types.Where(arg => arg.Name.Equals("Identify")).FirstOrDefault();

        }

        public static bool IsCHS(string filepath)
        {
            object result = InvokeMethod(new object[] { filepath });
            if (result == null) return false;
            bool.TryParse(result.ToString(), out bool v);
            return v;
        }



        public static bool IsHDV(string filepath)
        {
            object result = InvokeMethod(new object[] { filepath });
            if (result == null) return false;
            bool.TryParse(result.ToString(), out bool v);
            return v;
        }


        public static bool IsHDV(long filesize)
        {
            object result = InvokeMethod(new object[] { filesize });
            if (result == null) return false;
            bool.TryParse(result.ToString(), out bool v);
            return v;
        }

        private static string GetEng(string content)
        {
            object result = InvokeMethod(new object[] { content });
            return result == null ? "" : result.ToString();
        }


        private static string GetNum(string content)
        {
            object result = InvokeMethod(new object[] { content });
            return result == null ? "" : result.ToString();
        }


        public static int GetVideoType(string VID)
        {
            object result = InvokeMethod(new object[] { VID });
            if (result == null) return 0;
            int.TryParse(result.ToString(), out int v);
            return v;
        }


        public static string GetEuVID(string str)
        {
            object result = InvokeMethod(new object[] { str });
            return result == null ? "" : result.ToString();
        }


        public static string GetVID(string str)
        {
            object result = InvokeMethod(new object[] { str });
            return result == null ? "" : result.ToString();
        }

    }
}
