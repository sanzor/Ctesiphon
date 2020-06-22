using System;
using System.Collections.Generic;
using System.Text;

namespace WinformClient {
    [Serializable]
    public class Config {
        public string ServerUrl { get; set; }
        public static Config DEFAULT => new Config {
            ServerUrl = "ws://localhost:8600"
        };
    }
}
