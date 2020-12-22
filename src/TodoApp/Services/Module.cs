using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stl.DependencyInjection;
using Stl.Serialization;
using TodoApp.Abstractions;

namespace TodoApp.Services
{
    public class Module : ModuleBase
    {
        public Module(IServiceCollection services) : base(services) { }

        public override void ConfigureServices()
        {
            Services.TryAddSingleton(c => (Func<ISerializer<string>>) (
                () => new JsonNetSerializer(JsonNetSerializer.DefaultSettings)));
            Services.AttributeScanner().AddServicesFrom(GetType().Assembly);
        }
    }
}
