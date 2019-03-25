using System;
using System.Collections.Generic;
using System.Text;

namespace WS_Core.Bodys.Redis
{
    /// <summary>
    /// Redis 配置
    /// </summary>
    [Author("Linyee","2019-06-19")]
    public class RedisConfig
    {
        /// <summary>
        /// 默认实例
        /// </summary>
        public static RedisConfig Default = new RedisConfig();

        /// <summary>
        /// 超时
        /// </summary>
        public int timeout { get; set; } = 0;

        /// <summary>
        /// 从服务只读
        /// </summary>
        public string slave_read_only { get; set; } = "yes";

        /// <summary>
        /// 从服务只读
        /// </summary>
        public int databases { get; set; } = 16;

        /// <summary>
        /// 获取指定属性
        /// </summary>
        /// <param name="key"></param>
        /// <param name="key2"></param>
        /// <returns></returns>
        public ExecuteResult<object> GetValue(string key,string key2=null)
        {
            var realkey = key.Replace("-", "_");
            ExecuteResult<object> result = new ExecuteResult<object>();
            var p=this.GetType().GetProperty(realkey);
            if (p == null) return result.SetFail(key+"未找到指定对象名称，注意区分大小写");
            var val = p.GetValue(this);
            var valType = val.GetType();
            //子键
            if (valType == typeof(RedisInfo) && !string.IsNullOrEmpty(key2))
            {
                var p2= valType.GetProperty(key2);
                if (p2 == null) return result.SetFail(key2+"未找到指定对象名称，注意区分大小写");
                val = p2.GetValue(this);
            }

            result.SetOk().SetData(val.ToString());
            return result;
        }
    }

}
