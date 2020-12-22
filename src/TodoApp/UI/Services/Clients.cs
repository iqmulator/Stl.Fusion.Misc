using System;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Stl.Fusion.Client;
using TodoApp.Abstractions;

namespace TodoApp.UI.Services
{
    [RestEaseReplicaService(typeof(ITimeService), Scope = Module.ClientSideScope)]
    [BasePath("time")]
    public interface ITimeClient
    {
        [Get("get")]
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
    }
}
