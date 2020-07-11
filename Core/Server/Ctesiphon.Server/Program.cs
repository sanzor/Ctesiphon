using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ctesiphon.Conventions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Ctesiphon.Extensions;

namespace Ctesiphon.Server {
    public class Program {
       
        public static void Main(string[] args) {

            var logPath = Constants.LOG_FILE.ToCurrentAssemblyRootPath();

            Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logPath, outputTemplate: Constants.LOG_OUTPUT_TEMPLATE)
            .WriteTo.ColoredConsole(outputTemplate: Constants.LOG_OUTPUT_TEMPLATE)
            .Enrich.FromLogContext()
            .CreateLogger();
            CreateWebHostBuilder(args).Build().Run();
            Log.CloseAndFlush();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            var configPath = Constants.CONFIG_FILE.ToCurrentAssemblyRootPath();
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

