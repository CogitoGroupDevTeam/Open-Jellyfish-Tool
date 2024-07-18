
using System.Threading.Tasks;

namespace JellyfishTool.Modules.Features {

    public interface IFeatureModule {

        string Name { get; }

        Task Invoke();
    }
}