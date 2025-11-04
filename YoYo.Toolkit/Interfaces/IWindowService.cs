using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace YoYo.Toolkit.Interfaces
{
    /// <summary>
    /// author:TT
    /// time:2025/5/22 11:29:53
    /// desc:IWindowService
    /// </summary>
    public interface IWindowService
    {
        void RegisterWindow(Window window, string? windowId = null);
        void CloseWindow(string windowId);
    }
}
