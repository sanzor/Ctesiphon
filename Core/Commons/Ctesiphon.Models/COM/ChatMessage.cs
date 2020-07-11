using System;
using System.Collections.Generic;
using System.Text;

namespace Ctesiphon.Models {
    public class ChatMessage {
        public enum DISCRIMINATOR {
            SUBSCRIBE=0,
            UNSUBSCRIBE=1,
            SERVER=2,
            MESSAGE=3
        }
        
        public DISCRIMINATOR Kind { get; set; }
        public string SenderID { get; set; }
        public string Channel { get; set; }
        public DateTime IssuedAt { get; set; }
        public string Value { get; set; }
    }
}
