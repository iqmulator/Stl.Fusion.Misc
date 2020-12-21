using Microsoft.Extensions.DependencyInjection;

namespace TodoApp.Abstractions
{
    public abstract class ModuleBase
    {
        public IServiceCollection Services { get; init; }

        public abstract void ConfigureServices();
    }
}
