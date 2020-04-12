using System;
using System.Collections.Generic;
using System.Text;

namespace UnityChatApi.Models {
    public class Message {
        public string SenderID { get; set; }
        public string Channel { get; set; }
        public DateTime IssuedAt { get; set; }
        public string Value { get; set; }
    }
}
