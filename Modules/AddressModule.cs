
using System;
using System.Threading.Tasks;

using JellyfishTool.Config;
using JellyfishTool.Services;

namespace JellyfishTool.Modules {

    public class AddressModule {

        private readonly OpenJellyfishToolSettings settings;

        private readonly ClientService clientService;
        private readonly ConfigService configService;

        public AddressModule(
            OpenJellyfishToolSettings settings,
            ClientService clientService,
            ConfigService configService
        ) {
            this.settings = settings;
            this.clientService = clientService;
            this.configService = configService;
        }

        public async Task<bool> AssertAddress() {
            
            Console.WriteLine("Asserting Jellyfish address...");

            string address = settings.Jellyfish.Address;
            if (!string.IsNullOrEmpty(address)) {

                bool addressTestResult = await clientService.TestConnection(
                    address,
                    settings.Jellyfish.Proxy?.Address,
                    settings.Jellyfish.Proxy?.Port
                );
                if (addressTestResult) {
                    return true;
                }
            }

            Console.WriteLine("Jellyfish connection failed or not configured");
            await PromptAddress();

            return false;
        }

        private async Task PromptAddress() {

            Console.WriteLine("\nEnter Jellyfish URL:");
            string address = Console.ReadLine().Trim();
            settings.Jellyfish.Address = address;

            PromptProxy();

            bool connectionValid = await AssertAddress();
            if (!connectionValid) return;

            await configService.WriteSettings();
        }

        private void PromptProxy() {

            Console.WriteLine("Use an HTTP proxy? (yes\\no)");
            string response = Console.ReadLine().Trim();
            Console.WriteLine();

            //Check if proxy required
            const string YES = "yes";
            if (!response.Equals(YES, StringComparison.OrdinalIgnoreCase)) {
                settings.Jellyfish.Proxy = null;
                return;
            }
            settings.Jellyfish.Proxy ??= new ProxySettings();

            //Prompt address
            Console.WriteLine("Enter proxy address:");
            string address = Console.ReadLine().Trim();
            Console.WriteLine();

            settings.Jellyfish.Proxy.Address = address;

            //Prompt port
            int? port = null;
            while (port == null) {

                Console.WriteLine("Enter proxy port");
                string portString = Console.ReadLine().Trim();
                Console.WriteLine();

                bool success = int.TryParse(portString, out int portInt);
                if (success) {
                    port = portInt;
                } else {
                    Console.WriteLine("Port must be an integer");
                }
            }
            settings.Jellyfish.Proxy.Port = (int)port;
        }
    }
}