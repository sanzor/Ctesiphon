using System;
using System.Collections.Generic;
using System.Text;

namespace PubSubSharp.Models {
    public class ControlMessage {
        public string ClientId { get; set; }
        public string Channel { get; set; }
    }
}
