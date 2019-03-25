using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WS_Core.Bodys;
using WS_Core.Bodys.Json;
using WS_Core.Consts;
using WS_Core.Enums;
using WS_Core.BLL;
using WS_Server.Servers;
//using Microsoft.AspNetCore.Http;

namespace WS_Server.Servers
{
    /// <summary>
    /// WebSokcet连接信息
    /// Linyee 2018-06-03
    /// </summary>
    [Author("Linyee", "2019-01-21")]
    public partial class LinyeeWebSocketConnection: LinyeeWebSocketConnectionBase
    {
        #region "构造"

        /// <summary>
        /// 创建一个连接对象
        /// </summary>
        public LinyeeWebSocketConnection() : base()
        { }

        /// <summary>
        /// 创建一个连接对象
        /// </summary>
        public LinyeeWebSocketConnection(IMemberBll mbll) : base(mbll)
        { }

        /// <summary>
        /// 创建一个连接对象
        /// 如果 socketId 存在 会更新记录
        /// 如果 lid>0 且 type lid 相同 会更新记录
        /// </summary>
        /// <param name="socketId"></param>
        /// <param name="lid"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="ct"></param>
        /// <param name="currentSocket"></param>
        /// <param name="mbll"></param>
        public LinyeeWebSocketConnection(Guid socketId,long lid, int type, string name, CancellationToken ct, WebSocket currentSocket, IMemberBll mbll) : this(mbll)
        {
            this.ClientId = socketId;
            this.Client = currentSocket;

            this.TypeCode = type;
            this.LongId = lid;
            this.Id = (int)lid;
            this.Name = name;

            this.CancelToken = ct;

            OnlineSockets.AddOrUpdate(socketId, this, (key, value) => {
                var item = OnlineSockets[key];
                //更新时，先释放旧的资源
                item?.CloseMsg("您已在其它地方登录");

                return this;
            });
        }

        /// <summary>
        /// 克隆一个副本，不含计时器
        /// </summary>
        /// <returns></returns>
        internal new LinyeeWebSocketConnection Clone()
        {
            LinyeeWebSocketConnection obj = new LinyeeWebSocketConnection()
            {
                Client = this.Client,
                ClientId = this.ClientId,
                LongId = this.LongId,
                Id = this.Id,
                Name = this.Name,
                TypeCode = this.TypeCode,

                ConnectedTime = this.ConnectedTime,
                CancelToken=this.CancelToken,

                LastMsgTime=this.LastMsgTime,
            };
            return obj;
        }

        #endregion
    }
}
