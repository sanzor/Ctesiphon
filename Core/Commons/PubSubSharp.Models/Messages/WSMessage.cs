using System;
using System.Collections.Generic;
using System.Text;

namespace PubSubSharp.Models {
    public class WSMessage {
        public enum DISCRIMINATOR {
            SUBSCRIBE = 0,
            UNSUBSCRIBE = 1,
            SERVER = 2,
            MESSAGE = 3
        }

        public DISCRIMINATOR Kind { get; set; }
        public string Payload { get; set; }
       
    }
}
