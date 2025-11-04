using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoYo.Toolkit.Net.Http
{
    public abstract class BaseResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsOk { get; set; }
        /// <summary>
        /// 信息
        /// </summary>
        public string Msg { get; set; } = string.Empty;
        /// <summary>
        /// 反序列化
        /// </summary>
        public abstract void DeserializeObj();
        /// <summary>
        /// 数据
        /// </summary>
        public string Data { get; set; } = string.Empty;
    }
}
