
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using JellyfishTool.Config;
using JellyfishTool.Models;
using DTO = JellyfishTool.Models.DTO;

namespace JellyfishTool.Services {

    public class LicenseService {

        private const string ROUTE_GET_CERTIFICATE_AUTHORITIES = "api2/PKI-v1/CAs";

        private readonly OpenJellyfishToolSettings settings;
        private readonly ClientService clientFactory;

        public LicenseService(
            OpenJellyfishToolSettings settings,
            ClientService clientFactory
        ) {
            this.settings = settings;
            this.clientFactory = clientFactory;
        }

        public async Task<CertificateAuthority[]> GetAllCertificateAuthorities() {
            
            Console.WriteLine("Collecting all licensed certificate authorities...");

            //Prepare request
            string address = $"{settings.Jellyfish.Address}/{ROUTE_GET_CERTIFICATE_AUTHORITIES}";

            //Prepare client
            using HttpClient client = clientFactory.GetAuthenticatedClient();

            //Get CAs
            Console.WriteLine($"Submitting get: {address}");
            HttpResponseMessage response = await client.GetAsync(address);
            if (!response.IsSuccessStatusCode) {
                throw new InvalidOperationException("Get certificate authorities not success");
            }

            //Read response
            string casResponse = await response.Content.ReadAsStringAsync();
            DTO.CertificateAuthorityResponse cas = JsonSerializer.Deserialize<DTO.CertificateAuthorityResponse>(casResponse);

            //Convert
            CertificateAuthority[] authorities = CertificateAuthority.ParseDTO(cas);
            
            return authorities;
        }
    }
}