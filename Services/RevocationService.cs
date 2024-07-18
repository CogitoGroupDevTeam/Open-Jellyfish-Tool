
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using JellyfishTool.Config;
using JellyfishTool.Models;
using JellyfishTool.Models.DTO;

namespace JellyfishTool.Services {

    public class RevocationService {

        private const string ROUTE_REVOKE_CERT = "api2/PKI-v1/RevokeCert";

        private readonly OpenJellyfishToolSettings settings;
        private readonly ClientService clientFactory;

        public RevocationService(
            OpenJellyfishToolSettings settings,
            ClientService clientFactory
        ) {
            this.settings = settings;
            this.clientFactory = clientFactory;
        }

        public async Task RevokeCertificate(
            string caName,
            string serial,
            RevocationReason reason
        ) {

            Console.WriteLine("Submitting certificate revocation request...");

            //Prepare request
            string address = $"{settings.Jellyfish.Address}/{ROUTE_REVOKE_CERT}";
            RequestRevocationRequest model = new RequestRevocationRequest() {
                CaName = caName,
                Serial = serial,
                Reason = (int)reason
            };
            JsonContent payload = JsonContent.Create(model);

            //Prepare client
            using HttpClient client = clientFactory.GetAuthenticatedClient();

            //Submit request
            Console.WriteLine($"Submitting post: {address}");
            HttpResponseMessage response = await client.PostAsync(address, payload);
            if (!response.IsSuccessStatusCode) {
                throw new InvalidOperationException($"Post certificate revocation request response not success: {response.ReasonPhrase}");
            }

            //Successful revocations do not contain any content
            return;
        }
    }
}