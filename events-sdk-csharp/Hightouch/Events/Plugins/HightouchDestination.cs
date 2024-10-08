using System;
using Hightouch.Events.Utilities;
using Hightouch.Events.Serialization;
using Hightouch.Events.Sovran;

namespace Hightouch.Events.Plugins
{
    /// <summary>
    /// Hightouch Events plugin that is used to send events to Hightouch's tracking api, in the choice of region.
    /// How it works:
    /// <list type="number">
    /// <item><description>Plugin receives <c>apiHost</c> settings</description></item>
    /// <item><description>We store events into a file with the batch api format</description></item>
    /// <item><description>We upload events on a dedicated thread using the batch api</description></item>
    /// </list>
    /// </summary>
    public class HightouchDestination : DestinationPlugin, ISubscriber
    {
        private IEventPipeline _pipeline = null;

        public override string Key => "Hightouch.io";

        internal const string ApiHost = "apiHost";

        public override IdentifyEvent Identify(IdentifyEvent identifyEvent)
        {
            Enqueue(identifyEvent);
            return identifyEvent;
        }

        public override TrackEvent Track(TrackEvent trackEvent)
        {
            Enqueue(trackEvent);
            return trackEvent;
        }

        public override GroupEvent Group(GroupEvent groupEvent)
        {
            Enqueue(groupEvent);
            return groupEvent;
        }

        public override AliasEvent Alias(AliasEvent aliasEvent)
        {
            Enqueue(aliasEvent);
            return aliasEvent;
        }

        public override ScreenEvent Screen(ScreenEvent screenEvent)
        {
            Enqueue(screenEvent);
            return screenEvent;
        }

        public override PageEvent Page(PageEvent pageEvent)
        {
            Enqueue(pageEvent);
            return pageEvent;
        }

        public override void Configure(Analytics analytics)
        {
            base.Configure(analytics);

            // Add DestinationMetadata enrichment plugin
            Add(new DestinationMetadataPlugin());

            _pipeline = analytics.Configuration.EventPipelineProvider.Create(analytics, Key);

            analytics.AnalyticsScope.Launch(analytics.AnalyticsDispatcher, async () =>
            {
                await analytics.Store.Subscribe<System>(this, state => OnEnableToggled((System)state), true);
            });
        }

        public override void Update(Settings settings, UpdateType type)
        {
            base.Update(settings, type);

            JsonObject hightouchInfo = settings.Integrations?.GetJsonObject(Key);
            string apiHost = hightouchInfo?.GetString(ApiHost);
            if (apiHost != null && _pipeline != null)
            {
                _pipeline.ApiHost = apiHost;
            }
        }

        public override void Reset()
        {

        }

        public override void Flush() => _pipeline?.Flush();

        private void Enqueue<T>(T payload) where T : RawEvent
        {
            // TODO: filter out empty userid and traits values
            _pipeline?.Put(payload);
        }

        private void OnEnableToggled(System state)
        {
            if (state._enable)
            {
                _pipeline?.Start();
            }
            else
            {
                _pipeline?.Stop();
            }
        }
    }
}
