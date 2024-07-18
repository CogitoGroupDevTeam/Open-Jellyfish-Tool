
namespace JellyfishTool.Config {

    public class OpenJellyfishToolSettings {
        
        public const string OPEN_JELLYFISH_TOOL_SECTION = "OpenJellyfishTool";

        public JellyfishSettings Jellyfish { get; set; }

        public CryptoSettings Crypto { get; set; }
    }
}