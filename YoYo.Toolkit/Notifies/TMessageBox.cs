using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static YoYo.Toolkit.Notifies.TMessageBox;

namespace YoYo.Toolkit.Notifies
{
    public abstract class BaseMessageDialog : Window
    {
        public TMessageBoxResult MessageBoxResult { get; set; }
        public BaseMessageDialog() { }
        public BaseMessageDialog(string captionInfo, string msgInfo, MessageLevel level)
        {

        }
    }
    /// <summary>
    /// author:TT
    /// time:2023-03-26 16:33:48
    /// desc:TMessageBox
    /// </summary>
    public class TMessageBox
    {
        
        public enum TMessageBoxResult
        {
            None = 0,
            OK = 1,
            Cancel = 2,
            No = 3,
            Close = 4,
        }
        public enum MessageLevel
        {
            /// <summary>
            /// 信息
            /// </summary>
            Information = 0,
            /// <summary>
            /// 询问
            /// </summary>
            Question = 1,
            /// <summary>
            /// 警告
            /// </summary>
            Warning = 2,
            /// <summary>
            /// 错误
            /// </summary>
            Error = 3,
            /// <summary>
            /// 询问三种zhuangt
            /// </summary>
            QuestionNo = 4,
            YesNoCancel = 5,
        }
        static Type? windowType;
        public static void Init(Type baseMessageDialog)
        {
            windowType = baseMessageDialog;
        }
        static Window? GetWindow(string caption, string msg, MessageLevel level)
        {
            if (windowType == null) return null;
            // 获取带 string 参数的构造函数
            ConstructorInfo? ctor = windowType.GetConstructor(new[] { typeof(string), typeof(string), typeof(MessageLevel) });
            if (ctor != null)
            {
                object[] parameters = new object[] { caption, msg, level };
                Window window = (Window)ctor.Invoke(parameters);
                return window;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 弹窗
        /// </summary>
        /// <param name="caption">标题</param>
        /// <param name="msg">信息</param>
        /// <param name="level">级别</param>
        /// <returns></returns>
        public static TMessageBoxResult ShowMsg(string caption, string msg, MessageLevel level = MessageLevel.Information)
        {
            BaseMessageDialog? messageBox = GetWindow(caption, msg, level) as BaseMessageDialog;
            if (messageBox == null) return TMessageBoxResult.Cancel;
            messageBox.ShowDialog();
            return messageBox.MessageBoxResult;
        }

        /// <summary>
        ///弹窗
        /// </summary>
        /// <param name="msg">信息级别</param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static TMessageBoxResult ShowMsg(string msg, MessageLevel level = MessageLevel.Information)
        {
            TMessageBoxResult messageBoxResult = TMessageBoxResult.No;
            if (Application.Current.Dispatcher.CheckAccess())
            {
                BaseMessageDialog? messageBox = GetWindow("", msg, level) as BaseMessageDialog;
                if (messageBox == null) return TMessageBoxResult.Cancel;
                messageBox.ShowDialog();
                messageBoxResult = messageBox.MessageBoxResult;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {

                    BaseMessageDialog? messageBox = GetWindow("", msg, level) as BaseMessageDialog;
                    if (messageBox == null)
                        messageBoxResult = TMessageBoxResult.Cancel;
                    if (messageBox != null)
                    {
                        messageBox.ShowDialog();
                        messageBoxResult = messageBox.MessageBoxResult;
                    }
                });
            }
            return messageBoxResult;
        }

    }
}
