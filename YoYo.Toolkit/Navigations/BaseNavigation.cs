using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YoYo.Toolkit.Interfaces.Navigations;
using YoYo.Toolkit.Models.Navigations;

namespace YoYo.Toolkit.Navigations
{
    public abstract class BaseNavigation : INavigate
    {
        /// <summary>
        /// 导航名称
        /// </summary>
        public abstract string NavigationName { get; }

        IGetService _service;
        public BaseNavigation(IGetService getService)
        {
            _service = getService;
            InitView();
        }
        /// <summary>
        /// 日志
        /// </summary>
        /// <param name="msg"></param>
        public abstract void LogWrite(string? msg);
        /// <summary>
        /// 日志
        /// </summary>
        /// <param name="msg"></param>
        public abstract void LogWrite(string? msg, Exception? exception);
        /// <summary>
        /// 初始化
        /// </summary>
        public abstract void InitView();
        /// <summary>
        /// 导航合集
        /// </summary>
        public abstract List<NavigateModel> Navigates { get; }
        public IGetService Service => _service;
        public FrameworkElement? GetView<T>(string? name, T? param)
        {
            LogWrite($"导航到页面{name?.ToString()}");
            if (string.IsNullOrWhiteSpace(name)) return null;
            var nativeModel = Navigates.Find(p => p.ViewType == name);

            if (nativeModel == null) return null;
            var navigate = Service.GetService(nativeModel.VMType) as INavigateIn;
            try
            {


                if (navigate != null && param == null)
                {
                    navigate.NavigateIn();
                }
                else
                {
                    var navigateParam = Service.GetService(nativeModel.VMType) as INavigateIn<T>;
                    if (navigateParam != null)
                        navigateParam.NavigateIn(param);
                }

            }
            catch (Exception ex)
            {
                LogWrite($"导航发生错误", ex);
            }

            return Service.GetService(nativeModel.Type) as FrameworkElement;
        }
        /// <summary>
        /// 跳转
        /// </summary>
        /// <param name="name"></param>
        public void GoTo<T>(string? name, T? param)
        {
            if (Service.GetView() != null)
            {
                var navigate = Service?.GetView()?.DataContext as INavigateOut;
                if (navigate != null)
                    navigate.NavigateOut();

            }
            Service?.SetView(GetView(name, param));
        }
        public void GoTo(string? name)
        {
            FrameworkElement? view = Service.GetView();
            if (view != null)
            {
                INavigateOut? navigate = view?.DataContext as INavigateOut;
                if (navigate != null)
                    navigate.NavigateOut();
                //PreViewGoTo(view);
            }
            Service?.SetView(GetView<object>(name, null));
        }
        //public abstract void PreViewGoTo(FrameworkElement? currentView);
    }
}
