using System;
using System.Threading.Tasks;
using Pluralize.NET;
using Stl.Async;
using Stl.Fusion;
using Stl.Time;

namespace TodoApp.UI.Services
{
    // This service is local both on the server and on the client
    [ComputeService]
    public class MomentsAgoService
    {
        private IPluralize Pluralize { get; }
        private IMomentClock Clock { get; }

        public MomentsAgoService(IPluralize pluralize, IMomentClock? clock = null)
        {
            Pluralize = pluralize;
            Clock = clock ??= SystemClock.Instance;
        }

        [ComputeMethod]
        public virtual Task<string> GetMomentsAgoAsync(DateTime time)
        {
            var delta = Clock.Now.ToDateTime() - time;
            if (delta < TimeSpan.Zero)
                delta = TimeSpan.Zero;
            var (unit, unitName) = GetMomentsAgoUnit(delta);
            var unitCount = (int) (delta.TotalSeconds / unit.TotalSeconds);
            var pluralizedUnitName = Pluralize.Format(unitName, unitCount);
            var result = $"{unitCount} {pluralizedUnitName} ago";

            // Invalidate the result when it's supposed to change
            var delay = (unitCount + 1) * unit - delta;
            var computed = Computed.GetCurrent();
            Task.Delay(delay, default).ContinueWith(_ => computed!.Invalidate()).Ignore();

            return Task.FromResult(result);
        }

        public static (TimeSpan Unit, string UnitName) GetMomentsAgoUnit(TimeSpan delta)
        {
            if (delta.TotalSeconds < 60)
                return (TimeSpan.FromSeconds(1), "second");
            if (delta.TotalMinutes < 60)
                return (TimeSpan.FromMinutes(1), "minute");
            if (delta.TotalHours < 24)
                return (TimeSpan.FromHours(1), "hour");
            if (delta.TotalDays < 7)
                return (TimeSpan.FromDays(1), "day");
            return (TimeSpan.FromDays(7), "week");
        }
    }
}
