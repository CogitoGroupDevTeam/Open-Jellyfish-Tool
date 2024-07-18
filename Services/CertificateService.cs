
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using JellyfishTool.Config;
using JellyfishTool.Models.DTO;

namespace JellyfishTool.Services {

    public class CertificateService {

        private const string ROUTE_REQUEST_CERT = "api2/PKI-v1/RequestCert";

        private const string DISPOSITION_ISSUED = "issued";

        private readonly OpenJellyfishToolSettings settings;
        private readonly ClientService clientFactory;

        public CertificateService(
            OpenJellyfishToolSettings settings,
            ClientService clientFactory
        ) {
            this.settings = settings;
            this.clientFactory = clientFactory;
        }

        public async Task<RequestCertificateResponse> RequestCertificate(
            int caId,
            int licensedTemplateId,
            string certificateRequest
        ) {

            Console.WriteLine("Submitting PKCS10 certificate request...");

            //Prepare request
            string address = $"{settings.Jellyfish.Address}/{ROUTE_REQUEST_CERT}";
            RequestCertificateRequest model = new RequestCertificateRequest() {
                CaId = caId,
                LicensedTemplateId = licensedTemplateId,
                Csr = certificateRequest
            };
            JsonContent payload = JsonContent.Create(model);

            //Prepare client
            using HttpClient client = clientFactory.GetAuthenticatedClient();

            //Submit request
            Console.WriteLine($"Submitting post: {address}");
            HttpResponseMessage response = await client.PostAsync(address, payload);
            if (!response.IsSuccessStatusCode) {
                throw new InvalidOperationException($"Post certificate request response not success: {response.ReasonPhrase}");
            }

            //Read response
            string certResponse = await response.Content.ReadAsStringAsync();
            RequestCertificateResponse cert = JsonSerializer.Deserialize<RequestCertificateResponse>(certResponse);

            //Handle non issuance
            if (!cert.Disposition.Equals(DISPOSITION_ISSUED)) {
                throw new InvalidOperationException($"Certificate disposition not issued: ${cert.Disposition}, message: {cert.Message}");
            }

            return cert;
        }
    }
}