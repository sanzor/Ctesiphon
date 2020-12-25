using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace PubSubSharp.Server {
    public class Program {
        public static string ToCurrentAssemblyRootPath(string target) {
            var path = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().FullName).FullName, target);
            return path;
        }
        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            var configPath = ToCurrentAssemblyRootPath(Constants.CONFIG_FILE);
            IConfiguration config = new ConfigurationBuilder().AddJsonFile(configPath).Build();

            var con = config.GetSection("config").Get<Config>();
            var url = con.ServerUrl;
            var webhostbuilder = WebHost.CreateDefaultBuilder(args)

                .UseConfiguration(config)
                .UseUrls(url)
                .UseStartup<Startup>();

            return webhostbuilder;
        }

    }
}

