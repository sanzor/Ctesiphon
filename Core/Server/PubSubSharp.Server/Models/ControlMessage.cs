using System;
using System.Collections.Generic;
using System.Text;

namespace PubSubSharp.Server {
    public class ControlMessage {
        public string ClientId { get; set; }
        public string Channel { get; set; }
    }
}
