using System;
using System.Collections.Generic;
using System.Text;

namespace PubSubSharp.Server {
    public class WSMessage {
        public enum DISCRIMINATOR {
            CLIENT__SUBSCRIBE = 0,
            CLIENT_UNSUBSCRIBE = 1,
            CLIENT_MESSAGE = 3,
            CLIENT_GET_CHANNELS=4,
            SERVER__RESULT=100
        }
        
        public DISCRIMINATOR Kind { get; set; }
        public string Payload { get; set; }
       
    }
}
