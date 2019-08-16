using System;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Transport;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace KestrelRedisServer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// 监听端口
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            var port = 6379;
            int.TryParse( args.FirstOrDefault(p=>p.StartsWith("--Port:",StringComparison.OrdinalIgnoreCase))?.Substring(7),out port);
            int databases = 16;
            int.TryParse(args.FirstOrDefault(p => p.StartsWith("--Databases:", StringComparison.OrdinalIgnoreCase))?.Substring(12), out databases);

            return WebHost.CreateDefaultBuilder(args)
                .UseLibuv() //需要安装 Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv  Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions
                .UseKestrel(options =>
                {
                    options.ApplicationSchedulingMode = SchedulingMode.Inline;
                    //// HTTP 5000
                    //options.ListenLocalhost(5000);

                    // TCP 6379
                    options.ListenLocalhost(6379, builder => builder.UseConnectionHandler<RedisConnectionHandler>());

                    //options.ListenAnyIP(port, builder => builder.UseConnectionHandler<RedisConnectionHandler>());
                }).UseStartup<Startup>();
        }
    }
}
