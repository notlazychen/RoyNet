using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdventureGrainInterfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using Rabbit.WeiXin.Handlers;
using Rabbit.WeiXin.Handlers.Impl;

namespace RoyNet.Gateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});
            services.Configure<WechatConfig>(Configuration.GetSection("Wechat"));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddHttpClient();

            services.AddSingleton<IClusterClient>(provider=> 
            {
                var client = new ClientBuilder()
                    .UseLocalhostClustering()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev";
                        options.ServiceId = "AdventureApp";
                    })
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IRoomGrain).Assembly).WithReferences())
                    .Build();

                client.Connect().Wait();
                return client;
            });


            services.AddScoped<IWeiXinHandler>(provider => {
                IHandlerBuilder builder = new HandlerBuilder();
                builder
                    //.Use<SignatureCheckHandlerMiddleware>() //验证签名中间件。
                    .Use<CreateRequestMessageHandlerMiddleware>() //创建消息中间件（内置消息解密逻辑）。
                    .Use<SessionSupportHandlerMiddleware>() //会话支持中间件。
                    .Use<IgnoreRepeatMessageHandlerMiddleware>() //忽略重复的消息中间件。
                    .Use<WeMessageHandlerMiddleware>(provider.GetService<IClusterClient>()) //测试消息处理中间件。
                    .Use<GenerateResponseXmlHandlerMiddleware>(); //生成相应XML处理中间件（内置消息加密逻辑）。            
                    //.Use<AgentHandlerMiddleware>(new AgentRequestModel(new Uri("http://localhost:22479/Mutual")));
                IWeiXinHandler weiXinHandler = new DefaultWeiXinHandler(builder);
                return weiXinHandler;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
