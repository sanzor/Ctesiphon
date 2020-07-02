using Microsoft.Extensions.Configuration;
using PubSubSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using PubSubSharp.Extensions;
using System.Reactive.Linq;
using System.Threading;

namespace WinformClient {
    static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        public static Config GetConfiguration() {
            try {
                
              
                IConfiguration iconfig = new ConfigurationBuilder().AddJsonFile(Constants.CONFIG_FILE).Build();
                Config config = new Config { ServerUrl = iconfig.GetSection("config:server:url").Value };
                return config;
            } catch (Exception ex) {
                return Config.DEFAULT;
                
            }
           
        }
        [STAThread]
        static async Task Main() {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var config = GetConfiguration();
            ClientWebSocket socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(config.ServerUrl), CancellationToken.None);
           
            State state = new State(config, socket);

            Application.Run(new ChatForm(state));
        }
    }
}
