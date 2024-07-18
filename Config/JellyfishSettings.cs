
using System;
using System.Text.Json.Serialization;

namespace JellyfishTool.Config {

    public class JellyfishSettings {

        public string Address { get; set; }

        [JsonIgnore]
        public Uri UriAddress { get => new(Address); }

        public AuthSettings Auth { get; set; }
        
        public ProxySettings Proxy { get; set; }
    }
}