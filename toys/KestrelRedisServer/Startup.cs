using System;
using Linyee.StackExchange.Redis.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Server;

namespace KestrelRedisServer
{
    /// <summary>
    /// 启动组件
    /// </summary>
    public class Startup : IDisposable
    {
        /// <summary>
        /// 服务端实例
        /// </summary>
        private readonly RespServer _server = new LinyeeMemoryCacheRedisServer();

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //注入 服务端实例
            services.Add(new ServiceDescriptor(typeof(RespServer), _server));
        }

        /// <summary>
        /// 释放时
        /// </summary>
        public void Dispose() => _server.Dispose();

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// 配置时
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="lifetime"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            //Shutdown完成后继续任务 相当于关闭事件
            _server.Shutdown.ContinueWith((t, s) =>
            {
                Console.WriteLine("==Shutdown完成后进入");
                try
                {   // if the resp server is shutdown by a client: stop the kestrel server too
                    if (t.Result == RespServer.ShutdownReason.ClientInitiated)
                    {
                        Console.WriteLine("==结束应用");
                        ((IApplicationLifetime)s).StopApplication();
                    }
                }
                catch { }
            }, lifetime);

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            //输出服务端 状态
            app.Run(context => context.Response.WriteAsync(_server.GetStats()));
        }
    }
}
