
using System;
using System.Net;

using JellyfishTool.Config;

namespace JellyfishTool.Services {
    
    public class ProxyService {
        
        private readonly OpenJellyfishToolSettings settings;

        public ProxyService(OpenJellyfishToolSettings settings) {
            this.settings = settings;
        }

        public WebProxy GetProxy(string proxyAddress = null, int? proxyPort = null) {
            
            //Read address from args, fallback to config
            string address = proxyAddress;
            if (string.IsNullOrEmpty(proxyAddress)) {
                address = settings.Jellyfish.Proxy?.Address;
            }

            //Read port from args, fallback to config
            int? port = proxyPort;
            if (port == null) {
                port = settings.Jellyfish.Proxy?.Port;
            }

            //If address and port are both null, return a null proxy
            if (string.IsNullOrEmpty(address) && port == null) return null;

            //If one but not both are null, throw exception
            if (string.IsNullOrEmpty(address)) {
                throw new InvalidOperationException("Proxy address not specified");
            }
            if (port == null) {
                throw new InvalidOperationException("Proxy port not specified");
            }

            Console.WriteLine($"Using HTTP proxy: {address}:{port}");
            WebProxy proxy;
            try {
                proxy = new WebProxy(
                    address,
                    (int)port
                );
            } catch (UriFormatException) {
                throw new InvalidOperationException($"Invalid proxy address: {address}:{port}");
            }

            return proxy;
        }
    }
}