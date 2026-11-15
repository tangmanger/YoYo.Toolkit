using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YoYo.Toolkit.Interfaces;

namespace YoYo.Toolkit.Services
{
    public class WindowService : IWindowService
    {
        private readonly Dictionary<string, Window> _windows = new();

        public void RegisterWindow(Window window, string? windowId = null)
        {
            if (window is null || string.IsNullOrEmpty(windowId)) return;

            var id = windowId!;
            _windows[id] = window;

            // 同名 id 可以重新注册（关掉再开）。旧窗口后触发 Closed 时，
            // 只有字典里登记的仍是它自己才允许移除，否则会误删新窗口的登记。
            window.Closed += (_, _) =>
            {
                if (_windows.TryGetValue(id, out var current) && ReferenceEquals(current, window))
                    _windows.Remove(id);
            };
        }

        public void CloseWindow(string windowId)
        {
            if (string.IsNullOrEmpty(windowId)) return;
            if (_windows.TryGetValue(windowId, out var window) && window is not null)
            {
                window.Dispatcher.Invoke(window.Close);
            }
        }
    }
}
