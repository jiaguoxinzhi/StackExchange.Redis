using System;
using System.Collections.Generic;
using System.Text;

namespace WS_Core.Bodys
{
    #region  请求 基础
    /// <summary>
    /// 请求 基础
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public class MQReQuestBase : MQReQuestSponseBase
    {
        /// <summary>
        /// 构造
        /// </summary>
        public MQReQuestBase()
        {
            Body = new object();
        }

        /// <summary>
        /// 访问路径
        /// </summary>
        public string path;
    }

    /// <summary>
    /// 请求 基础
    /// </summary>
    [Author("Linyee", "2019-01-28")]
    public class MQReQuestBase<T> : MQReQuestSponseBase<T>
         where T : QuestBodyBase, new()
    {
        /// <summary>
        /// 构造
        /// </summary>
        public MQReQuestBase()
        {
            Body = new T();
        }

        /// <summary>
        /// 访问路径
        /// </summary>
        public string path;
    }
    #endregion
}
