using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace YoYo.Toolkit.Interfaces.Navigations
{
    public interface INavigate
    {
        /// <summary>
        /// 页面窗口
        /// </summary>
        IGetService Service { get; }
        /// <summary>
        /// 导航到
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="name">页面key</param>
        /// <param name="param">参数</param>
        void GoTo<T>(string? name, T? param);
        /// <summary>
        /// 导航
        /// </summary>
        /// <param name="name">页面key</param>
        void GoTo(string? name);
        /// <summary>
        /// 获取View
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="name">页面key</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        FrameworkElement? GetView<T>(string? name, T? param);
    }
}
