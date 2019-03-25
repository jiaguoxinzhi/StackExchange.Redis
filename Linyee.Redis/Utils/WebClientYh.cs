using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace WS_Core.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class WebClientYh:System.Net.WebClient
    {
        /// <summary>
        /// cookies
        /// </summary>
        public CookieContainer CookieContainer { get; } = new CookieContainer();

        /// <summary>
        /// 超时 毫秒
        /// </summary>
        public int Timeout { get; set; } = 10000;

        /// <summary>
        /// 超时 毫秒
        /// </summary>
        public IPAddress ipAddress { get; set; } = IPAddress.Any;//网卡上的IP

        /// <summary>
        /// 获取请求对象时
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            var webRequest = base.GetWebRequest(address);
            webRequest.Timeout = Timeout;
            if (webRequest is HttpWebRequest)
            {
                HttpWebRequest httpRequest = webRequest as HttpWebRequest;
                httpRequest.CookieContainer = CookieContainer;

                httpRequest.ServicePoint.BindIPEndPointDelegate+= (servicePoint, remoteEndPoint, retryCount) =>
                {
                    return new IPEndPoint(ipAddress, 0);
                };
            }

            return webRequest;
        }
    }
}