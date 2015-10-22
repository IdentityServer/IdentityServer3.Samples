using IdentityServer3.Core.Services;
using System.Threading.Tasks;
using IdentityServer3.Core.Events;
using Serilog;

namespace SelfHost
{
    class SeqEventService : IEventService
    {
        static readonly ILogger Log;

        static SeqEventService()
        {
            Log = new LoggerConfiguration()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();
        }

        public Task RaiseAsync<T>(Event<T> evt)
        {
            Log.Information("{Id}: {Name} / {Category} ({EventType}), Context: {@context}, Details: {@details}",
                evt.Id,
                evt.Name,
                evt.Category,
                evt.EventType,
                evt.Context,
                evt.Details);

            return Task.FromResult(0);
        }
    }
}