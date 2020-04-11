using System;
using System.Collections.Generic;
using System.Text;

namespace UnityChatApi.Models {
    public class Message {
        public DateTime IssuedAt { get; set; }
        public string Value { get; set; }
    }
}
