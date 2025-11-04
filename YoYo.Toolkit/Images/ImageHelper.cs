using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace YoYo.Toolkit.Images
{
    public static class ImageHelper
    {
        /// <summary>
        /// 获取图片
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static BitmapImage? GetBitmapImage(this string filePath)
        {
            if (!File.Exists(filePath)) return null;
            using (MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(filePath)))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}
