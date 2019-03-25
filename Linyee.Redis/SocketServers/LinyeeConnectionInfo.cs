using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WS_Server.SocketServers
{
    /// <summary>
    /// 连接信息
    /// </summary>
    [Author("Linyee", "2019-03-19")]
    public class LinyeeConnectionInfo : ConnectionInfo
    {
        /// <summary>
        /// 连接id
        /// </summary>
        public override string Id { get; set ; }
        /// <summary>
        /// 远程地址
        /// </summary>
        public override IPAddress RemoteIpAddress { get; set; }
        /// <summary>
        /// 远程端口
        /// </summary>
        public override int RemotePort { get; set; }
        /// <summary>
        /// 本地地址
        /// </summary>
        public override IPAddress LocalIpAddress { get; set; }
        /// <summary>
        /// 本地端口
        /// </summary>
        public override int LocalPort { get; set; }
        /// <summary>
        /// 读书信息
        /// </summary>
        public override X509Certificate2 ClientCertificate { get; set; }

        public override Task<X509Certificate2> GetClientCertificateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
