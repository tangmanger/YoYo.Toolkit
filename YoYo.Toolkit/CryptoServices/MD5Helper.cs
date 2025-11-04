using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace YoYo.Toolkit.CryptoServices
{
    public class MD5Helper
    {/// <summary>
     /// 对字符串进行MD5加密
     /// </summary>
     /// <param name="strText"></param>
     /// <returns></returns>
        public static string Md5(string strText)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(strText));
                string output = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return output;
            }
        }
        public static byte[]? Md5ToByte(string strText)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(strText));
                return hashBytes;
            }
        }
        public static string CalculateMD5Hash(string filePath)
        {
            try
            {
                // 创建一个MD5对象  
                using (MD5 md5Hash = MD5.Create())
                {
                    // 打开文件并计算其哈希值  
                    using (FileStream stream = File.OpenRead(filePath))
                    {
                        // 计算文件的哈希值  
                        byte[] hash = md5Hash.ComputeHash(stream);

                        // 创建一个StringBuilder对象来收集字节并创建字符串  
                        StringBuilder builder = new StringBuilder();

                        // 遍历哈希值的每个字节并格式化为十六进制字符串  
                        for (int i = 0; i < hash.Length; i++)
                        {
                            builder.Append(hash[i].ToString("x2"));
                        }

                        // 返回十六进制字符串  
                        return builder.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                // 处理任何可能发生的异常  
                throw new Exception($"Error calculating MD5 hash for file {filePath}: {ex.Message}");
            }
        }
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="strText">加密内容</param>
        /// <param name="strint">16位或32位Md5值</param>
        /// <param name="isUpper">是否大小写</param>
        /// <returns></returns>
        public static string Md5(string strText, int strint, bool isUpper)
        {
            if (strint == 16)
            {
                var output = Md5ToByte(strText);
                if (output == null) return "";
                if (isUpper)
                    return BitConverter.ToString(output, 4, 8).Replace("-", "").ToUpper();
                return BitConverter.ToString(output, 4, 8).Replace("-", "").ToLower();
            }
            if (strint != 32) return "";
            {
                var output = Md5ToByte(strText);
                if (output == null) return "";
                if (isUpper)
                    return BitConverter.ToString(output).Replace("-", "").ToUpper();
                return BitConverter.ToString(output).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// 对字符串进行MD5加密，返回不移除-的Md5值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isUpper">是否大小写</param>
        /// <returns></returns>
        public static string Md5NoReplace(string str, bool isUpper)
        {
            byte[]? targets = Md5ToByte(str);
            if (targets == null) return "";
            if (isUpper)
                return BitConverter.ToString(targets).ToUpper();
            return BitConverter.ToString(targets).ToLower();
        }



        public static string Md5Encrypt(string strSource, int length)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(strSource);
            byte[]? hashValue = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            switch (length)
            {
                case 16:
                    for (int i = 4; i < 12; i++)
                        sb.Append(hashValue[i].ToString("x2"));
                    break;
                case 32:
                    for (int i = 0; i < 16; i++)
                    {
                        sb.Append(hashValue[i].ToString("x2"));
                    }
                    break;
                default:
                    for (int i = 0; i < hashValue.Length; i++)
                    {
                        sb.Append(hashValue[i].ToString("x2"));
                    }
                    break;
            }
            return sb.ToString().ToUpper();
        }
    }
}
