using global::System;
using global::System.IO;
using global::System.Linq;
using global::System.Collections.Generic;
using Hightouch.Events.Serialization;
using Hightouch.Events.Utilities;
using Hightouch.Events.Sovran;

namespace Hightouch.Events.Plugins
{
    public class StartupQueue : Plugin, ISubscriber
    {
        private readonly object _gate = new object();
        private readonly Queue<RawEvent> _pending = new Queue<RawEvent>();
        private readonly Dictionary<string, string> _saved = new Dictionary<string, string>();
        private readonly HashSet<string> _waiting = new HashSet<string>();
        private string _directory;
        private string _replaying;
        private volatile bool _running;

        public override PluginType Type => PluginType.Before;

        public override void Configure(Analytics analytics)
        {
            base.Configure(analytics);
            if (analytics.Configuration.StorageProvider is DefaultStorageProvider provider)
                _directory = Path.Combine(provider.PersistentDataPath, "hightouch.data", analytics.Configuration.WriteKey, "startup");

            lock (_gate)
            {
                if (_directory != null)
                {
                    foreach (var file in (Directory.Exists(_directory) ? Directory.GetFiles(_directory, "*.json") : new string[0]).OrderBy(File.GetCreationTimeUtc))
                    {
                        try { _pending.Enqueue(Decode(File.ReadAllText(file))); }
                        catch (Exception e) { analytics.ReportInternalError(AnalyticsErrorType.StorageInvalid, e); }
                    }
                }
                else
                {
                    var saved = analytics.Storage.Read(StorageConstants.StartupEvents);
                    if (saved != null)
                    {
                        foreach (var pair in JsonUtility.FromJson<Dictionary<string, string>>(saved))
                        {
                            _saved.Add(pair.Key, pair.Value);
                            try { _pending.Enqueue(Decode(pair.Value)); }
                            catch (Exception e) { analytics.ReportInternalError(AnalyticsErrorType.StorageInvalid, e); }
                        }
                    }
                }
            }

            analytics.AnalyticsScope.Launch(analytics.AnalyticsDispatcher, async () =>
                await analytics.Store.Subscribe<System>(this, state => RunningUpdate((System)state)));
        }

        internal static RawEvent Decode(string json)
        {
            var record = JsonUtility.FromJson<JsonObject>(json);
            foreach (var key in record.Keys.Where(keyName => record[keyName] is JsonNull).ToArray())
                record.Remove(key);
            var user = record.GetString("userId");
            var name = record.GetString("name");
            var category = record.GetString("category");
            var properties = record.GetJsonObject("properties");
            RawEvent result;
            switch (record.GetString("type"))
            {
                case "track": result = new TrackEvent(record.GetString("event"), properties); break;
                case "identify": result = new IdentifyEvent(user, record.GetJsonObject("traits")); break;
                case "group": result = new GroupEvent(record.GetString("groupId"), record.GetJsonObject("traits")); break;
                case "alias": result = new AliasEvent(user, record.GetString("previousId")); break;
                case "page": result = new PageEvent(category, name, properties); break;
                case "screen": result = new ScreenEvent(category, name, properties); break;
                default: throw new InvalidDataException("Unknown startup event type.");
            }
            result.UserId = user;
            result.AnonymousId = record.GetString("anonymousId");
            result.MessageId = Guid.Parse(record.GetString("messageId")).ToString();
            result.Timestamp = record.GetString("timestamp");
            result.Context = record.GetJsonObject("context");
            result.Integrations = record.GetJsonObject("integrations");
            result.Metrics = record.GetJsonArray("metrics");
            var metadata = record.GetJsonObject("_metadata");
            if (metadata != null) result._metadata = JsonUtility.FromJson<DestinationMetadata>(metadata.ToString());
            return result;
        }

        internal bool Capture(RawEvent e)
        {
            if (_running || e == null) return false;
            lock (_gate)
            {
                if (_running) return false;
                _pending.Enqueue(e);
                if (e.Enrichment != null)
                {
                    Analytics.ReportInternalError(AnalyticsErrorType.PluginError, message:
                        "Startup events with enrichment callbacks can only be replayed in this session.");
                    return true;
                }

                try
                {
                    var json = JsonUtility.ToJson(e);
                    if (_directory != null)
                    {
                        Directory.CreateDirectory(_directory);
                        var file = Path.Combine(_directory, Guid.Parse(e.MessageId) + ".json");
                        File.WriteAllText(file + ".tmp", json);
                        File.Move(file + ".tmp", file);
                    }
                    else
                    {
                        _saved[e.MessageId] = json;
                        Analytics.Storage.WritePrefs(StorageConstants.StartupEvents, JsonUtility.ToJson(_saved));
                    }
                }
                catch (Exception ex) { Analytics.ReportInternalError(AnalyticsErrorType.StorageUnableToWrite, ex); }
                return true;
            }
        }

        public override RawEvent Execute(RawEvent e) => Capture(e) ? null : e;

        internal void RunningUpdate(System state)
        {
            lock (_gate)
            {
                _running = state._running;
                while (_running && state._enable && _pending.Count > 0)
                {
                    var e = _pending.Peek();
                    _replaying = e.MessageId;
                    try
                    {
                        Analytics.Timeline.Process(e);
                        if (!_waiting.Contains(e.MessageId) && e.Enrichment == null) Acknowledge(e.MessageId);
                        _pending.Dequeue();
                    }
                    finally { _replaying = null; }
                }
            }
        }

        internal void WillWrite(string id)
        {
            lock (_gate) { if (_replaying != null && _replaying == id) _waiting.Add(id); }
        }

        internal void DidWrite(string id)
        {
            lock (_gate) { if (_waiting.Remove(id)) Acknowledge(id); }
        }

        private void Acknowledge(string id)
        {
            if (_directory != null) File.Delete(Path.Combine(_directory, Guid.Parse(id) + ".json"));
            else if (_saved.Remove(id)) Analytics.Storage.WritePrefs(StorageConstants.StartupEvents, JsonUtility.ToJson(_saved));
        }
    }
}
