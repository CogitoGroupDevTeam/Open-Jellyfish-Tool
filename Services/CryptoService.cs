
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Utilities.IO.Pem;

namespace JellyfishTool.Services {

    public class CryptoService {

        public static async Task<string> CollectCertificateSigningRequest(string input) {

            string fileContent = await FileSystemService.ReadAsText(input);

            byte[] pemDecodeResult = PemDecode(fileContent);
            if (IsPkcs10(pemDecodeResult)) {
                return fileContent;
            }

            Console.WriteLine("Input is unrecognizable");
            return null;
        }

        public static async Task<X509Certificate2> CollectCertificate(string input) {

            byte[] fileContent = await FileSystemService.ReadAsBytes(input);
            if (fileContent == null) {
                Console.WriteLine("Input is unrecognizable");
                return null;
            }
            
            X509Certificate2 cert;
            try {
                cert = new X509Certificate2(fileContent);
            } catch (Exception ex) {
                Console.WriteLine($"File is not an X509 certificate: {ex.Message}");
                return null;
            }

            return cert;
        }

        public static X509Certificate2 MergeCertKey(X509Certificate2 cert, AsymmetricAlgorithm key) {
            
            if (key is RSA rsa) {
                return cert.CopyWithPrivateKey(rsa);
            }

            if (key is ECDsa ecc) {
                return cert.CopyWithPrivateKey(ecc);
            }

            throw new InvalidOperationException("Asymmetric algorithm was not RSA or ECDsa");
        }

        public static async Task<RSA> CollectRSAKey(string input) {

            string fileContent = await FileSystemService.ReadAsText(input);
            if (fileContent == null) {
                Console.WriteLine("Input is unrecognizable");
                return null;
            }            

            RSA key = RSA.Create();
            try {
                key.ImportFromPem(fileContent);
            } catch (Exception ex) {
                Console.WriteLine($"File is not an RSA key: {ex.Message}");
                return null;
            }

            return key;
        }

        public static async Task<ECDsa> CollectECDsaKey(string input) {

            string fileContent = await FileSystemService.ReadAsText(input);
            if (fileContent == null) {
                Console.WriteLine("Input is unrecognizable");
                return null;
            }

            ECDsa key = ECDsa.Create();
            try {
                key.ImportFromPem(fileContent);
            } catch (Exception ex) {
                Console.WriteLine($"File is not an ECDsa key: {ex.Message}");
                return null;
            }

            return key;
        }

        public static bool IsPkcs10(byte[] input) {

            try {

                Pkcs10CertificationRequest request = new Pkcs10CertificationRequest(input);
                return request.Verify();

            } catch (Exception) {
                Console.WriteLine($"Input is not PKCS10");
                return false;
            }
        }

        public static string PemEncode(string type, byte[] content) {

            //Create pem objects
            PemObject pemObject = new PemObject(type, content);

            //Create pem writer
            MemoryStream stream;
            using (stream = new MemoryStream()) {
                using StreamWriter textWriter = new StreamWriter(stream);
                using PemWriter pemWriter = new PemWriter(textWriter);

                //Write to stream
                pemWriter.WriteObject(pemObject);
            }

            //Read text from stream
            byte[] pemBytes = stream.ToArray();
            string pemString = Encoding.UTF8.GetString(pemBytes);

            return pemString;
        }

        public static byte[] PemDecode(string input) {

            try {
                
                //Create pem reader
                using TextReader stringReader = new StringReader(input);
                using PemReader pemReader = new PemReader(stringReader);

                //Read pem
                PemObject pem = pemReader.ReadPemObject();
                byte[] encoded = pem.Content;

                return encoded;

            } catch (Exception) {
                Console.WriteLine($"Input is not PEM");
                return null;
            }
        }
    }
}