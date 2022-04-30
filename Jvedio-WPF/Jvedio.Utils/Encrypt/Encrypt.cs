﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Utils.Encrypt
{


    /// <summary>
    /// AES 加密，加密有道、百度AI的 key 到本地
    /// </summary>
    public static class Encrypt
    {
        /// <summary>
        ///  AES 加密
        /// </summary>
        /// <param name="str">明文（待加密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        public static string AesEncrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);
            RijndaelManaged rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray);
        }

        /// <summary>
        ///  AES 解密
        /// </summary>
        /// <param name="str">明文（待解密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        public static string AesDecrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Convert.FromBase64String(str);

            RijndaelManaged rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }


        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }

        public static string GetFileMD5(string filename)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        public static string FasterMd5(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            long minLength = 1024;
            int BYTES_TO_READ = 64;
            FileInfo fileInfo = new FileInfo(filePath);
            double length = fileInfo.Length;
            if (length <= minLength) return GetFileMD5(filePath);
            byte[] B = new byte[BYTES_TO_READ * 3];


            using (FileStream fs = fileInfo.OpenRead())
            {
                byte[] a = new byte[BYTES_TO_READ];
                byte[] b = new byte[BYTES_TO_READ];
                byte[] c = new byte[BYTES_TO_READ];
                fs.Read(a, 0, BYTES_TO_READ);
                fs.Seek((long)(length / 2), SeekOrigin.Begin);
                fs.Read(b, 0, BYTES_TO_READ);
                fs.Seek((long)(length - BYTES_TO_READ - 1), SeekOrigin.Begin);
                fs.Read(c, 0, BYTES_TO_READ);
                a.CopyTo(B, 0);
                b.CopyTo(B, BYTES_TO_READ);
                c.CopyTo(B, BYTES_TO_READ * 2);
            }
            string str = BitConverter.ToString(B);
            str = length + str;
            return CalculateMD5Hash(str);
        }

        public static string FasterDirMD5(List<string> filePathsInOneDir)
        {
            if (filePathsInOneDir == null || filePathsInOneDir.Count == 0) return null;
            // 获取第 1,2,n/2,-2,-1 个文件的哈希
            int count = filePathsInOneDir.Count;
            string[] idx = filePathsInOneDir.ToArray();
            if (count >= 5)
            {
                idx = new string[] { filePathsInOneDir[0],
                    filePathsInOneDir[1], filePathsInOneDir[count / 2],
                    filePathsInOneDir[count - 2], filePathsInOneDir[count - 1] };
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < idx.Length; i++)
            {
                string hash = FasterMd5(idx[i]);
                builder.Append(hash);
            }
            return CalculateMD5Hash(builder.ToString());
        }


        /// <summary>
        /// 计算所有文件的MD5
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static string GetFilesMD5(string[] files)
        {
            var total = "";
            foreach (var item in files)
            {
                total += GetFileMD5(item).Substring(0, 5);
            }
            return CalculateMD5Hash(total);
        }

        public static long GetDirectorySize(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }

        public static string GetDirectoryMD5(string folderPath)
        {
            return CalculateMD5Hash(Path.GetDirectoryName(folderPath) + GetDirectorySize(folderPath));
        }
    }
}
