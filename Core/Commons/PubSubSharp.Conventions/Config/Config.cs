using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PubSubSharp.Conventions {
    public class Config {
        [JsonPropertyName("serverUrl")]
        public string ServerUrl { get; set; }

        [JsonPropertyName("redis")]
        public Redis Redis { get; set; }
    }
}
