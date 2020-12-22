using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stl.OS;

namespace TodoApp.UI
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            if (OSInfo.Kind != OSKind.WebAssembly)
                throw new ApplicationException("This app runs only in browser.");

            var hostBuilder = WebAssemblyHostBuilder.CreateDefault(args);
            var uiModule = new Module(hostBuilder);
            uiModule.ConfigureServices();
            hostBuilder.RootComponents.Add<App>("#app");
            var host = hostBuilder.Build();

            var runTask = host.RunAsync();
            Task.Run(async () => {
                // We "manually" start IHostedServices here, because Blazor host doesn't do this.
                var hostedServices = host.Services.GetRequiredService<IEnumerable<IHostedService>>();
                foreach (var hostedService in hostedServices)
                    await hostedService.StartAsync(default);
            });
            return runTask;
        }
    }
}
