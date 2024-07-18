
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using JellyfishTool.Config;

namespace JellyfishTool.Services {

    public class ConfigService {

        private static readonly string APPSETTINGS_FILENAME = "appsettings.json";

        private readonly OpenJellyfishToolSettings settings;

        public ConfigService(
            OpenJellyfishToolSettings settings
        ) {
            this.settings = settings;
        }

        public static OpenJellyfishToolSettings AssertSettings(OpenJellyfishToolSettings settings) {

            settings ??= new OpenJellyfishToolSettings();

            settings.Jellyfish ??= new JellyfishSettings();
            settings.Jellyfish.Auth ??= new AuthSettings();

            return settings;
        }

        public async Task WriteSettings() {

            Console.WriteLine("Saving updated appsettings...");

            var wrapper = new {
                OpenJellyfishTool = settings
            };
            string settingsString = JsonSerializer.Serialize(wrapper);

            //Write to disc
            string exePath = AppContext.BaseDirectory;
            string appSettingsPath = Path.Combine(exePath, APPSETTINGS_FILENAME);
            await File.WriteAllTextAsync(appSettingsPath, settingsString);
        }
    }
}
