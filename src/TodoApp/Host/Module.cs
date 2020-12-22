using Microsoft.Extensions.DependencyInjection;
using Stl.DependencyInjection;
using TodoApp.Abstractions;

namespace TodoApp.Host
{
    public class Module : ModuleBase
    {
        public Module(IServiceCollection services) : base(services) { }

        public override void ConfigureServices()
            => Services.AttributeScanner().AddServicesFrom(GetType().Assembly);
    }
}
