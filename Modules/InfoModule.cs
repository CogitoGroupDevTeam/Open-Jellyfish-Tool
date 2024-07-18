
using System;

using JellyfishTool.Services;

namespace JellyfishTool.Modules {

    public class InfoModule {

        public static void PrintInto() {
            
            string intro = string.Format(@"
▄▄▄▄▄▄▄▄▄▄▄▄▄▄
█▒ ▄▄▄▄▄▄▄▄▄ █▒   
█▒ █ ▄▄▄▄▄ █ █▒   {0}: Jellyfish CLI API Client
█▒ █ █ ▄▄▄▄█ █▒   Copyright: Cogito Group {1}
█▒ █ █ █▄▄ █ █▒   Version: {2}
█▒ █ █▄▄▄▄▄█ █▒
█▒ █▄▄▄▄▄▄▄▄▄█▒
",
                IdentityService.GetProductName(),
                DateTime.Now.Year,
                IdentityService.GetProductVersion()
            );

            Console.WriteLine(intro);
        }
    }
}