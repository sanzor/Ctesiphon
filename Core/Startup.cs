using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers();
            ConnectionMultiplexer mux = ConnectionMultiplexer.Connect(Constants.REDIS_CONNECTION);
            services.AddSingleton(mux);
        }
        public void Configure(IApplicationBuilder app) {
            app.UseRouting();
            app.UseWebSockets();
            app.MapWhen(y => y.WebSockets.IsWebSocketRequest, a => a.UseMiddleware<SocketWare>());
        }
    }

}
