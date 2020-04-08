using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ASPT.Conventions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ASPT.Server {
    public class Program {
        public static string ToCurrentAssemblyRootPath(string target) {
            var path = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().FullName).FullName, target);
            return path;
        }
        public static void Main(string[] args) {

            var logPath = ToCurrentAssemblyRootPath(Constants.LOG_FILE);

            Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logPath, outputTemplate: Constants.LOG_OUTPUT_TEMPLATE)
            .WriteTo.ColoredConsole(outputTemplate: Constants.LOG_OUTPUT_TEMPLATE)
            .Enrich.FromLogContext()
            .CreateLogger();
            CreateWebHostBuilder(args).Build().Run();
           Log.CloseAndFlush();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            var configPath = ToCurrentAssemblyRootPath(Constants.CONFIG_FILE);
          //  Log.Information($"Using config at path: {configPath}");
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

