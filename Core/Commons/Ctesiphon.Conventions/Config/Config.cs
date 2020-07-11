using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Ctesiphon.Conventions {
    public class Config {
        [JsonPropertyName("serverUrl")]
        public string ServerUrl { get; set; }
        [JsonPropertyName("swagger")]
        public Swagger Swagger { get; set; }

        [JsonPropertyName("redis")]
        public Redis Redis { get; set; }
    }
}
