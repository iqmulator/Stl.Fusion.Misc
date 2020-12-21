using Stl.DependencyInjection;
using TodoApp.Abstractions;

namespace TodoApp.Services
{
    public class Module : ModuleBase
    {
        public override void ConfigureServices()
            => Services.AttributeBased().AddServicesFrom(GetType().Assembly);
    }
}
