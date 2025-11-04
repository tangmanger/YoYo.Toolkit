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

            if (!string.IsNullOrEmpty(windowId) && window != null)
            {
                _windows[windowId] = window;
                window.Closed += (s, e) => _windows.Remove(windowId);
            }
        }

        public void CloseWindow(string windowId)
        {
            if (_windows.TryGetValue(windowId, out var window))
            {
                window.Dispatcher.Invoke(window.Close);
            }
        }

        
    }
}
