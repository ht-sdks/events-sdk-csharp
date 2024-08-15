using System;
using System.Collections.Generic;
using Segment.Serialization;

namespace Hightouch.Events.Plugins
{
    public class DestinationMetadataPlugin : Plugin
    {
        public override PluginType Type => PluginType.Enrichment;

        private Settings _settings;

        public override void Update(Settings settings, UpdateType type)
        {
            base.Update(settings, type);
            _settings = settings;
        }

        public override RawEvent Execute(RawEvent incomingEvent)
        {
            // TODO: Precompute the metadata instead of compute it for every events once we have Sovran changed to synchronize mode
            HashSet<string> bundled = new HashSet<string>();
            HashSet<string> unbundled = new HashSet<string>();

            foreach (Plugin plugin in Analytics.Timeline._plugins[PluginType.Destination]._plugins)
            {
                if (plugin is DestinationPlugin destinationPlugin && !(plugin is HightouchDestination) && destinationPlugin._enabled)
                {
                    bundled.Add(destinationPlugin.Key);
                }
            }

            // All active integrations, not in `bundled` are put in `unbundled`
            foreach (string integration in _settings.Integrations?.Keys ?? Array.Empty<string>())
            {
                if (integration != "Hightouch.io" && !bundled.Contains(integration))
                {
                    unbundled.Add(integration);
                }
            }

            // All unbundledIntegrations not in `bundled` are put in `unbundled`
            JsonArray unbundledIntegrations =
                _settings.Integrations?.GetJsonObject("Hightouch.io")?.GetJsonArray("unbundledIntegrations") ??
                new JsonArray();
            foreach (JsonElement integration in unbundledIntegrations)
            {
                string content = ((JsonPrimitive)integration).Content;
                if (!bundled.Contains(content))
                {
                    unbundled.Add(content);
                }
            }

            incomingEvent._metadata = new DestinationMetadata
            {
                Bundled = CreateJsonArray(bundled),
                Unbundled = CreateJsonArray(unbundled),
                BundledIds = new JsonArray()
            };

            return incomingEvent;
        }

        private JsonArray CreateJsonArray(IEnumerable<string> list)
        {
            var jsonArray = new JsonArray();
            foreach (string value in list)
            {
                jsonArray.Add(value);
            }

            return jsonArray;
        }
    }
}
