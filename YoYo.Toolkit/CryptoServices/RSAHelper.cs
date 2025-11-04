using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace YoYo.Toolkit.CryptoServices
{
    public class RSAHelper
    {



        #region 加解密
        /// <summary>
        /// 加密向量
        /// </summary>
        static byte[] iv = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07 };
        /// <summary>
        /// 密钥
        /// </summary>
       public static string? EncryptKey { get; set; }

        public static void SetIV(byte[] bytes)
        {
            iv = bytes;
        }
        /// <summary>
        /// 加密算法
        /// </summary>
        /// <param name="normalTxt"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AESEncrypt(string normalTxt, string key = "")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                key = EncryptKey;
            }
            var bytes = Encoding.Default.GetBytes(normalTxt);
            SymmetricAlgorithm des = Aes.Create();
            des.Key = Encoding.UTF8.GetBytes(key);
            des.IV = iv;
            using (MemoryStream ms = new MemoryStream())
            {
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(bytes, 0, bytes.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
        }
        /// <summary>
        /// 解密算法
        /// </summary>
        /// <param name="securityTxt"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AESDecrypt(string securityTxt, string key = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    key = EncryptKey;
                }
                var bytes = Convert.FromBase64String(securityTxt);
                SymmetricAlgorithm des = Aes.Create();
                des.Key = Encoding.UTF8.GetBytes(key);
                des.IV = iv;
                using (MemoryStream ms = new MemoryStream())
                {
                    CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
                    cs.Write(bytes, 0, bytes.Length);
                    cs.FlushFinalBlock();

                    var Result = ms.ToArray();
                    return Encoding.Default.GetString(Result);
                }
            }
            catch (FormatException fex)
            {
                throw new FormatException(fex.Message, fex);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }

        #endregion
    }
}
