using Microsoft.Extensions.DependencyInjection;

namespace TodoApp.Abstractions
{
    public interface IModule
    {
        IServiceCollection Services { get; }
        void ConfigureServices();
    }
}
