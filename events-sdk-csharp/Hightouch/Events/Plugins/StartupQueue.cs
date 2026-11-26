using global::System.Collections.Generic;
using Hightouch.Events.Concurrent;
using Hightouch.Events.Utilities;
using Hightouch.Events.Sovran;

namespace Hightouch.Events.Plugins
{
    /// <summary>
    /// Analytics plugin to manage started state of analytics client
    /// All events will be held in an in-memory queue until started state is enabled, and once enabled
    /// events will be replayed into the analytics timeline
    /// </summary>
    public class StartupQueue : Plugin, ISubscriber
    {
        private static readonly int s_maxSize = 1000;
        private bool _running;
        private readonly Queue<RawEvent> _queuedEvents = new Queue<RawEvent>();

        public override PluginType Type => PluginType.Before;

        public override void Configure(Analytics analytics)
        {
            base.Configure(analytics);
            analytics.AnalyticsScope.Launch(analytics.AnalyticsDispatcher, async () => await analytics.Store.Subscribe<System>(this, state => RunningUpdate((System)state)));
        }

        public override RawEvent Execute(RawEvent incomingEvent)
        {
            lock (_queuedEvents)
            {
                if (_running || incomingEvent == null) return incomingEvent;
                if (_queuedEvents.Count >= s_maxSize) _queuedEvents.Dequeue();
                _queuedEvents.Enqueue(incomingEvent);
                return null;
            }
        }

        private void RunningUpdate(System state)
        {
            RawEvent[] pending;
            lock (_queuedEvents)
            {
                _running = state._running;
                if (!_running || _queuedEvents.Count == 0) return;
                pending = _queuedEvents.ToArray();
                _queuedEvents.Clear();
            }
            Analytics.AnalyticsScope.Launch(Analytics.AnalyticsDispatcher, () =>
            {
                foreach (var e in pending)
                {
                    if (Analytics.Enable) Analytics.Timeline.Process(e);
                }
            });
        }
    }
}
