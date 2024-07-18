
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using JellyfishTool.Config;
using JellyfishTool.Services;

namespace JellyfishTool.Modules {

    public class CryptographicModule {

        private readonly OpenJellyfishToolSettings settings;

        private readonly ConfigService config;
        private readonly SessionService session;

        public CryptographicModule(
            OpenJellyfishToolSettings settings,
            ConfigService config,
            SessionService session
        ) {
            this.settings = settings;
            this.config = config;
            this.session = session;
        }

        public async Task<bool> AssertCrypto() {

            Console.WriteLine("Asserting cryptographic configuration...");

            string certPath = settings.Crypto?.CmcSignerCertificatePath;
            string keyPath = settings.Crypto?.CmcSignerKeyPath;

            if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(keyPath)) {

                X509Certificate2 cert = await CryptoService.CollectCertificate(certPath);

                AsymmetricAlgorithm key;
                if (cert?.PublicKey.GetRSAPublicKey() != null) {
                    key = await CryptoService.CollectRSAKey(keyPath);
                } else {
                    key = await CryptoService.CollectECDsaKey(keyPath);
                }

                if (cert != null && key != null) {

                    X509Certificate2 merged = CryptoService.MergeCertKey(cert, key);
                    session.SignerCertificate = merged;
                    session.SignerKey = key;

                    return true;
                }
            }

            Console.WriteLine("Cryptographic configuration not configured");
            return await PromptCmcSigner();
        }

        private async Task<bool> PromptCmcSigner() {
            
            Console.WriteLine("Configure CMC signer certificate now? (yes\\no)");
            string response = Console.ReadLine().Trim();
            Console.WriteLine();

            //Check if CMC signer  will be configured now or later
            const string YES = "yes";
            if (!response.Equals(YES, StringComparison.OrdinalIgnoreCase)) {
                settings.Crypto = null;
                return false;
            }
            settings.Crypto ??= new CryptoSettings();

            //Prompt certificate
            X509Certificate2 cert = await PromptCertificate();
            Console.WriteLine();

            //Prompt key
            await PromptKey(cert);
            Console.WriteLine();

            //Save
            await config.WriteSettings();
            return true;
        }

        private async Task<X509Certificate2> PromptCertificate() {

            string certString = string.Empty;
            X509Certificate2 cert = null;
            while (cert == null) {

                Console.WriteLine("Enter CMC signer certificate path:");                
                certString = Console.ReadLine().Trim();
                Console.WriteLine();

                cert = await CryptoService.CollectCertificate(certString);

                if (cert.PublicKey.GetRSAPublicKey() != null) {

                } else if (cert.PublicKey.GetECDsaPublicKey() != null) {

                } else {
                    Console.WriteLine("Public key algorithm not supported, must be RSA or ECDsa");
                    cert = null;
                }
            }

            settings.Crypto.CmcSignerCertificatePath = certString;
            return cert;
        }

        private async Task PromptKey(X509Certificate2 cert) {

            string keyString = null;
            AsymmetricAlgorithm priv = null;
            while (priv == null) {

                Console.WriteLine("Enter enrollment agent private key path:");
                keyString = Console.ReadLine().Trim();
                Console.WriteLine();

                if (cert.PublicKey.GetRSAPublicKey() != null) {
                    priv = await CryptoService.CollectRSAKey(keyString);
                } else {
                    priv = await CryptoService.CollectECDsaKey(keyString);
                }
            }

            settings.Crypto.CmcSignerKeyPath = keyString;
        }
    }
}