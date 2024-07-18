
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using JellyfishTool.Config;
using JellyfishTool.Models.DTO;

namespace JellyfishTool.Services {

    public class ClientService {

        private const string JELLYFISH_TOKEN_HEADER = "X-Jellyfish-Token";
        
        private const string ROUTE_CHECK_CONNECTION_PATH = "jf_versions.json";
        private const string ROUTE_CHECK_API_KEY_PATH = "api2/Authentication-v1/check-session";

        private readonly OpenJellyfishToolSettings settings;
        private readonly ProxyService proxyService;
        private readonly SessionService session;

        public ClientService(
            OpenJellyfishToolSettings settings,
            ProxyService proxyService,
            SessionService session
        ) {
            this.settings = settings;
            this.proxyService = proxyService;
            this.session = session;
        }

        public async Task<bool> TestConnection(string jellyfishAddress, string proxyAddress = null, int? proxyPort = null) {

            Console.WriteLine("Testing connection to Jellyfish...");

            //Prepare request
            Console.WriteLine($"Using address: '{jellyfishAddress}'");
            string address = $"{jellyfishAddress}/{ROUTE_CHECK_CONNECTION_PATH}";

            //Prepare proxy
            WebProxy proxy = null;
            if (proxyAddress != null && proxyPort != null) {
                
                Console.WriteLine($"Using proxy: '{proxyAddress}:{proxyPort}'");
                try {

                    proxy = proxyService.GetProxy(proxyAddress, proxyPort);

                } catch (InvalidOperationException ex) {

                    Console.WriteLine($"Proxy validation failure: {ex.Message}");
                    return false;
                } 

            } else {
                Console.WriteLine("Proxy not specified");
            }

            //Prepare client
            using HttpClient client = GetClient(proxy);

            //Test connection
            HttpResponseMessage response;
            try {
                Console.WriteLine($"Submitting get: {address}");
                response = await client.GetAsync(address);
            } catch (Exception ex) {
                Console.WriteLine($"Jellyfish connection error: {ex.Message}");
                return false;
            }

            if (!response.IsSuccessStatusCode) {
                return false;
            }

            //Read response
            string versionResponse = await response.Content.ReadAsStringAsync();
            JellyfishVersionsResponse versions = JsonSerializer.Deserialize<JellyfishVersionsResponse>(versionResponse);

            Console.WriteLine($"Successful connection to Jellyfish: {versions.Product}");
            return true;
        }

        public async Task<bool> TestApiKey(string apiKey) {

            Console.WriteLine("Testing API key against Jellyfish...");

            //Prepare request
            string address = $"{settings.Jellyfish.Address}/{ROUTE_CHECK_API_KEY_PATH}";
            StringContent payload = new StringContent(string.Empty);

            //Prepare client
            using HttpClient client = GetAuthenticatedClient(apiKey);

            //Test key
            HttpResponseMessage response;
            try {
                Console.WriteLine($"Submitting post: {address}");
                response = await client.PostAsync(address, payload);
            } catch (Exception ex) {
                Console.WriteLine($"API key validation error: {ex.Message}");
                return false;
            }

            if (!response.IsSuccessStatusCode) {
                return false;
            }

            //Read response
            string sessionResponse = await response.Content.ReadAsStringAsync();
            CheckSessionResponse keySession = JsonSerializer.Deserialize<CheckSessionResponse>(sessionResponse);

            Console.WriteLine("API key authorized");

            session.TenantId = keySession.TenantId.First();
            Console.WriteLine($"Connected to TenantId: {session.TenantId}");

            session.UserId = keySession.UserId.First();
            Console.WriteLine($"Connected as UserId: {session.UserId}");

            return true;
        }
        
        public HttpClient GetAuthenticatedClient(string apiKey = null) {

            Console.WriteLine("Preparing authenticated HTTP Client...");

            //Prepare key
            string key = apiKey;
            if (string.IsNullOrEmpty(key)) {
                key = settings.Jellyfish.Auth.ApiKey;
            }
            if (string.IsNullOrEmpty(key)) {
                throw new InvalidOperationException("API key not specified and not configured in Jellyfish Auth appsettings");
            }
            
            //Prepare proxy
            WebProxy proxy = proxyService.GetProxy();
            HttpClient client = GetClient(proxy);

            //Append key header
            client.DefaultRequestHeaders.Add(
                JELLYFISH_TOKEN_HEADER,
                key
            );

            return client;
        }

        private static HttpClient GetClient(WebProxy proxy) {
            
            HttpClient client = new(
                new HttpClientHandler() {
                    Proxy = proxy
                }, 
                true
            );

            return client;
        }
    }
}