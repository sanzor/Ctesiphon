using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace UnityChatApi.Conventions {
    public class Config {
        [JsonPropertyName("serverUrl")]
        public string ServerUrl { get; set; }
        [JsonPropertyName("swagger")]
        public Swagger Swagger { get; set; }
    }
}
