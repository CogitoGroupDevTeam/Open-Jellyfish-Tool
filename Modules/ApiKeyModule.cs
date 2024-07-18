
using System;
using System.Threading.Tasks;

using JellyfishTool.Config;
using JellyfishTool.Services;

namespace JellyfishTool.Modules {

    public class ApiKeyModule {

        private readonly OpenJellyfishToolSettings settings;

        private readonly ClientService clientService;
        private readonly ConfigService configService;

        public ApiKeyModule(
            OpenJellyfishToolSettings settings,
            ClientService clientService,
            ConfigService configService
        ) {
            this.settings = settings;
            this.clientService = clientService;
            this.configService = configService;
        }

        public async Task<bool> AssertApiKey() {

            Console.WriteLine("Asserting API key...");

            string key = settings.Jellyfish.Auth?.ApiKey;
            if (!string.IsNullOrEmpty(key)) {
                
                bool keyTestResult = await clientService.TestApiKey(key);
                if (keyTestResult) {
                    return true;
                }
            }

            Console.WriteLine("API key expired or not found");
            Console.WriteLine();

            await PromptApiKey();

            return false;
        }

        private async Task PromptApiKey() {
            
            Console.WriteLine("Enter a Jellyfish API key:");
            string key = Console.ReadLine().Trim();
            Console.WriteLine();
            
            settings.Jellyfish.Auth.ApiKey = key;

            bool keyValid = await AssertApiKey();
            if (!keyValid) return;

            await configService.WriteSettings();
        }
    }
}