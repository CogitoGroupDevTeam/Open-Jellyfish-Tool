
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using JellyfishTool.Config;
using JellyfishTool.Modules;
using JellyfishTool.Modules.Features;
using JellyfishTool.Services;

IConfigurationBuilder builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true, false);
IConfiguration config = builder.Build();

OpenJellyfishToolSettings settings = config
    .GetSection(OpenJellyfishToolSettings.OPEN_JELLYFISH_TOOL_SECTION)
    .Get<OpenJellyfishToolSettings>();

settings = ConfigService.AssertSettings(settings);

ServiceProvider provider = new ServiceCollection()

    //Config
    .AddSingleton(settings)

    //Services
    .AddSingleton<RuntimeService>()
    .AddSingleton<SessionService>()

    .AddTransient<ConfigService>()
    .AddTransient<ProxyService>()
    .AddTransient<ClientService>()
    .AddTransient<LicenseService>()
    .AddTransient<CertificateService>()
    .AddTransient<RevocationService>()
    .AddTransient<CmcService>()

    //Base Modules
    .AddTransient<AddressModule>()
    .AddTransient<ApiKeyModule>()
    .AddTransient<CryptographicModule>()
    .AddTransient<MainModule>()

    //Feature Modules
    .AddTransient<IFeatureModule, CertificateRequestModule>()
    .AddTransient<IFeatureModule, CertificateRevocationModule>()
    .AddTransient<IFeatureModule, CmcCertificateRequestModule>()
    .AddTransient<IFeatureModule, CmcCertificateRevocationModule>()
    .AddTransient<IFeatureModule, ExitModule>()

    .BuildServiceProvider();

MainModule main = provider.GetService<MainModule>();
await main.Run();