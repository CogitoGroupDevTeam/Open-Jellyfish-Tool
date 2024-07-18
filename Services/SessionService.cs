
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace JellyfishTool.Services {

    public class SessionService {

        public string TenantId { get; set; }
        public string UserId { get; set; }

        public X509Certificate2 SignerCertificate { get; set; }
        public AsymmetricAlgorithm SignerKey { get; set; }
    }
}