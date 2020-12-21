using Stl.DependencyInjection;
using TodoApp.Abstractions;

namespace TodoApp.Host
{
    public class Module : ModuleBase
    {
        public override void ConfigureServices()
            => Services.AttributeBased().AddServicesFrom(GetType().Assembly);
    }
}
