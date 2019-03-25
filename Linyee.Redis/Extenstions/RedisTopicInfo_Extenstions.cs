using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WS_Server.Servers;

namespace WS_Server.Servers
{
    /// <summary>
    /// 主题 扩展
    /// </summary>
    [Author("Linyee","2019-03-12")]
    public static class RedisTopicInfo_Extenstions
    {
        /// <summary>
        /// 批量移除关注
        /// 自动判断是否需要移除主题、换房主
        /// </summary>
        public static ExecuteResult<ConcurrentDictionary<string, RedisTopicInfo>> RemoveSub(this ConcurrentDictionary<string, RedisTopicInfo> RedisTopic, LinyeeWebSocketConnectionBase client)
        {
            ExecuteResult<ConcurrentDictionary<string, RedisTopicInfo>> result = new ExecuteResult<ConcurrentDictionary<string, RedisTopicInfo>>();
            result.Data=(RedisTopic);

            if (RedisTopic == null) result.SetFail("主题字典不能为null");
            if (client == null ) return result.SetFail("连接数据不能为空");

            foreach (var topic in RedisTopic.Values)
            {
                //客户端已关注时
                if (topic.SubIds.Contains(client.LongId))
                {
                    var res = RedisTopic.RemoveSub(topic, client);
                    result.Subs.Add(res);
                }
            }

            return result;
        }

        /// <summary>
        /// 移除关注
        /// 自动判断是否需要移除主题、换房主
        /// </summary>
        public static ExecuteResult<RedisTopicInfo> RemoveSub(this ConcurrentDictionary<string, RedisTopicInfo> RedisTopic, RedisTopicInfo topic, LinyeeWebSocketConnectionBase client)
        {
            ExecuteResult<RedisTopicInfo> result = new ExecuteResult<RedisTopicInfo>();
            result.Data = topic;
            if (RedisTopic == null) return result.SetFail("主题字典不能为null");
            if (topic == null) return result.SetFail("主题数据不能为null");
            if (client == null) return result.SetFail("连接数据不能为空");
            RedisTopicInfo oitem = null;

            topic.SubIds.Remove(client.LongId);
            if (topic.SubIds.Count < 1 && topic.TypeCode < 1000000000000)//非系统主题
            {
                RedisTopic.TryRemove(topic.Topic, out oitem);//移除主题
                result.SetOk("");
            }
            else if (topic.SubIds.Count > 0)
            {
                if (topic.LongId == client.LongId) topic.LongId = topic.SubIds.FirstOrDefault();//换房主
            }

            return result;
        }
    }
}
