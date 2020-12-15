using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PubSubSharp.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PubSubSharp.DataAccess;
using System.Net.WebSockets;
using StackExchange.Redis;
using System.Threading;
using System.Text;

namespace PubSubSharp.Server {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            try {
                Config config = this.Configuration.GetSection("config").Get<Config>();
                services.Configure<Config>(this.Configuration.GetSection("config"));
                services.AddControllers();
                RedisStore store = new RedisStore(config.Redis.Con);
                services.AddSingleton(store);
                services.AddSwaggerGen(x => {
                    x.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo {
                        Title = config.Swagger.Title,
                        Version = config.Swagger.Version,
                        Description = config.Swagger.Description,
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact {
                            Name = "Bercovici Adrian",
                            Email = "bercovici.adrian.simon@gmail.com"
                        },
                    });
                    x.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement());

                    x.AddSecurityDefinition("sec", new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                        Description = "logging system for the Leplace App",
                        Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows {
                            Implicit = new Microsoft.OpenApi.Models.OpenApiOAuthFlow(),

                        }
                    });
                });
            } catch (Exception ex) {

                throw;
            }

        }
        public class SocketWrapper {
            public SocketWrapper(WebSocket socket, Guid guid) {
                this.socket = socket;
                this.Guid = guid;
            }
            public readonly WebSocket socket;
            public readonly Guid Guid;
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            //ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect("localhost:6379");

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(x => {
                x.SwaggerEndpoint("/swagger/v1/swagger.json", "versions");
            });
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
            app.UseWebSockets();
            app.MapWhen(y => y.WebSockets.IsWebSocketRequest, a => a.UseMiddleware<SocketWare>());
            //app.Use(async (con, req) => {

            //    if (!con.WebSockets.IsWebSocketRequest) {
            //        await req();
            //    }

            //    //accepting ws
            //    WebSocket socket = await con.WebSockets.AcceptWebSocketAsync();
            //    var wrapper = new SocketWrapper(socket, Guid.NewGuid());
            //    Console.WriteLine($"Main Guid :{wrapper.Guid}");
            //    //connecting to redis
              

            //    ISubscriber subscriber = connectionMultiplexer.GetSubscriber();
                


            //    //subscribing to redis channel 
            //    await subscriber.SubscribeAsync("mychannel",
            //          async (chan, val) => {
            //              Console.WriteLine($"Callback Guid :{wrapper.Guid}");
            //              await wrapper.socket.SendAsync(Encoding.UTF8.GetBytes(val),
            //                       WebSocketMessageType.Text,
            //                       true,
            //                       CancellationToken.None); //throws right here when user reconnects and publishes from the main Task the first message to redis
            //          }, CommandFlags.FireAndForget);
            //    var database = connectionMultiplexer.GetDatabase();
            //    byte[] buffer = new byte[1024];

            //    //loop for publishing client ws messages to redis channel
            //    while (true) {
            //        WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);
            //        if (result.MessageType == WebSocketMessageType.Close) {
            //            await subscriber.UnsubscribeAsync("mychannel");
            //            return;
            //        }
            //        var data = Encoding.UTF8.GetString(buffer[0..result.Count]);

            //        await database.PublishAsync("mychannel", data);
            //    }
            //});

        }
    }

}
