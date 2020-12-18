using System;
using System.Collections.Generic;
using System.Text;

namespace PubSubSharp.Models {
    public class ChatMessage {
        public string SenderId { get; set; }
        public string Channel { get; set; }
        public string Message { get; set; }
    }
}
