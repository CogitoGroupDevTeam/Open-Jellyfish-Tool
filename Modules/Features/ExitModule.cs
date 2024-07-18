
using System;
using System.Threading.Tasks;

using JellyfishTool.Services;

namespace JellyfishTool.Modules.Features {

    public class ExitModule : IFeatureModule
    {
        private readonly RuntimeService runtime;

        public string Name => "Exit"; 

        public ExitModule(
            RuntimeService runtime
        ) {
            this.runtime = runtime;
        }

        public Task Invoke()
        {
            Console.WriteLine("Terminating...");
            runtime.Running = false;

            return Task.CompletedTask;
        }
    }
}