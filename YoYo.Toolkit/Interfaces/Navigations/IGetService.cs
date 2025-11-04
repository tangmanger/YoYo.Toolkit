using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace YoYo.Toolkit.Interfaces.Navigations
{
    public interface IGetService
    {
        /// <summary>
        /// 获取服务
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        object GetService(Type? t);
        /// <summary>
        /// 获取页面
        /// </summary>
        /// <returns></returns>
        FrameworkElement? GetView();
        /// <summary>
        /// 设置页面
        /// </summary>
        /// <param name="frameworkElement"></param>
        void SetView(FrameworkElement? frameworkElement);
    }
    /// <summary>
    /// author:TT
    /// time:2021/5/10 16:56:27
    /// desc:INavigation
    /// </summary>
    public interface INavigateIn<T>
    {
        /// <summary>
        /// 进入
        /// </summary>
        void NavigateIn(T? param);


    }

    /// <summary>
    /// 无参数导航接口
    /// </summary>
    public interface INavigateIn
    {
        /// <summary>
        /// 进入
        /// </summary>
        void NavigateIn();


    }
    /// <summary>
    /// 无参数导航出
    /// </summary>
    public interface INavigateOut
    {
        /// <summary>
        /// 离开
        /// </summary>
        void NavigateOut();
    }
}
