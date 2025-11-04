using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoYo.Toolkit.Models.Navigations
{
    public class NavigateModel
    {
        /// <summary>
        /// 界面
        /// </summary>
        public string? ViewType { get; set; }
        /// <summary>
        /// 实例类型
        /// </summary>
        public Type? Type { get; set; }
        /// <summary>
        /// vm类型
        /// </summary>
        public Type? VMType { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? Name { get; set; }
    }
}
