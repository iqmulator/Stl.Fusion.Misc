using Microsoft.Extensions.DependencyInjection;

namespace TodoApp.Abstractions
{
    public abstract class ModuleBase : IModule
    {
        public IServiceCollection Services { get; }

        protected ModuleBase(IServiceCollection services)
            => Services = services;

        public abstract void ConfigureServices();
    }
}
