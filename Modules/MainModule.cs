
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JellyfishTool.Services;
using JellyfishTool.Modules.Features;

namespace JellyfishTool.Modules {

    public class MainModule {

        private readonly RuntimeService runtime;

        private readonly AddressModule addressMod;
        private readonly ApiKeyModule apiKeyMod;
        private readonly CryptographicModule cryptoMod;

        private readonly IEnumerable<IFeatureModule> features;

        public MainModule(
            RuntimeService runtime,
            AddressModule addressMod,
            ApiKeyModule apiKeyMod,
            CryptographicModule cryptoMod,
            IEnumerable<IFeatureModule> features
        ) {
            this.runtime = runtime;
            this.addressMod = addressMod;
            this.apiKeyMod = apiKeyMod;
            this.cryptoMod = cryptoMod;

            this.features = features;
        }

        public async Task Run() {

            InfoModule.PrintInto();

            Console.WriteLine("Initializing...");
            Console.WriteLine();
            
            await addressMod.AssertAddress();
            Console.WriteLine();
            await apiKeyMod.AssertApiKey();
            Console.WriteLine();
            await cryptoMod.AssertCrypto();
            Console.WriteLine();

            Console.WriteLine("Systems online");
            Console.WriteLine();

            while(runtime.Running) {
                
                await PromptFeature();
                Console.WriteLine();
            }
        }

        private async Task PromptFeature() {

            Func<IFeatureModule, string> getName = feature => feature.Name;
            IFeatureModule feature = PromptOption<IFeatureModule>(features, getName);
            await feature.Invoke();
        }

        public static T PromptOption<T>(IEnumerable<T> options, Func<T, string> getName) {

            //Write options
            Console.WriteLine($"Select an option [1-{options.Count()}]:");            
            for (int i = 0; i < options.Count(); i++) {
                Console.WriteLine($"- {i +1}: {getName(options.ElementAt(i))}");
            }

            //Prompt feature election
            int? election = null;
            while (election == null) {

                string electionString = Console.ReadLine();
                Console.WriteLine();

                bool success = int.TryParse(electionString, out int electionInt);
                if (!success) {
                    Console.WriteLine("Election must be an integer");
                    continue;
                }

                if (electionInt < 1 || electionInt > options.Count()) {
                    Console.WriteLine($"Election must be within range of: [1-{options.Count()}]");
                    continue;
                }
                
                election = electionInt;
            }

            return options.ElementAt((int)election -1);
        }
    }
}