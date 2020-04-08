using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ASPT.Conventions {
    public class Swagger {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}
