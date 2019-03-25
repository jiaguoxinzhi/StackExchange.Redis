using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.Internal.Networking;
using StackExchange.Redis.Server;

namespace KestrelRedisServer
{
    /// <summary>
    /// 连接处理
    /// </summary>
    public class RedisConnectionHandler : ConnectionHandler
    {
        /// <summary>
        /// 服务端
        /// </summary>
        private readonly RespServer _server;
        /// <summary>
        /// 注入 服务端实例
        /// </summary>
        /// <param name="server"></param>
        public RedisConnectionHandler(RespServer server) => _server = server;
        /// <summary>
        /// 发生连接时
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            try
            {
                await _server.RunClientAsync(connection.Transport).ConfigureAwait(false);
            }
            catch (IOException io) when (io.InnerException is UvException uv && uv.StatusCode == -4077)
            { } //swallow libuv disconnect
        }
    }
}
