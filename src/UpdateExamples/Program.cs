using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;

namespace UpdateExamples
{
    public class Program
    {
        private static async Task Main()
        {
            var s = new ServiceCollection();
            s.AddFusion().AddComputeService<MyService>().AddComputeService<MyAggregateService>();
            var sb = s.BuildServiceProvider();

            var stateFactory = sb.GetRequiredService<IStateFactory>();
            var myAggregateService = sb.GetRequiredService<MyAggregateService>();


            var live = stateFactory.NewLive<string>((_, ct) => myAggregateService.Aggregate());

            live.Updated += (s, _) => Console.WriteLine(s.Value);

            await Task.Delay(5000);

            myAggregateService.UiInput = "New Value";

            await Task.Delay(5000);
        }
    }

    public class MyAggregateService
    {
        private readonly MyService _myService;
        private string _uiInput = "Some Value";

        public MyAggregateService(MyService myService)
        {
            _myService = myService;
        }

        public string UiInput
        {
            get => _uiInput;
            set
            {
                _uiInput = value;
                Computed.Invalidate(() => Aggregate());
            }
        }

        [ComputeMethod]
        public virtual async Task<string> Aggregate()
        {
            var serviceResult = await _myService.GetWebApiResultAsync();

            return $"{serviceResult} - {_uiInput}";
        }
    }

    public class MyService
    {
        [ComputeMethod]
        public virtual async Task<string> GetWebApiResultAsync()
        {
            var computed = Computed.GetCurrent();
            using var _ = Computed.Suppress(); // Suppresses dependency capture
            var result = await GetWebApiResultAsyncImpl();

            async Task MaybeInvalidate()
            {
                await Task.Delay(2000); // The delay here should be >= AutoInvalidateTime value above
                var mustInvalidate = true;

                try
                {
                    var newValue = await GetWebApiResultAsyncImpl();
                    mustInvalidate = IsChanged((string) computed.Value, newValue);
                }
                catch (Exception e)
                {
                }
                finally
                {
                    // One of actions below should be called no matter what,
                    // otherwise GetWebApiResultAsync result will stay the same forever
                    if (mustInvalidate)
                        computed.Invalidate();
                    else
                        Task.Run(MaybeInvalidate);
                }
            }

            Task.Run(MaybeInvalidate);

            return result;
        }

        [ComputeMethod(KeepAliveTime = 1, AutoInvalidateTime = 1)] // Caches WebAPI call result for 1 second
        protected virtual async Task<string> GetWebApiResultAsyncImpl()
        {
            // Run the actual WebAPI request here
            Console.WriteLine("Executing GetWebApiResultAsyncImpl");

            return "Test";
        }

        private bool IsChanged(string a, string b)
        {
            return a == b;
        }
    }
}
