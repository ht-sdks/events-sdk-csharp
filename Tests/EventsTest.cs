using System.Collections.Generic;
using Moq;
using Hightouch.Events;
using Hightouch.Events.Utilities;
using Hightouch.Events.Serialization;
using Tests.Utils;
using Xunit;

namespace Tests
{
    public class EventsTest
    {
        private readonly Analytics _analytics;

        private Settings? _settings;

        private readonly Mock<StubEventPlugin> _plugin;

        public EventsTest()
        {
            _settings = JsonUtility.FromJson<Settings?>(
                "{\"integrations\":{\"Hightouch.io\":{\"apiKey\":\"1vNgUqwJeCHmqgI9S1sOm9UHCyfYqbaQ\"}},\"plan\":{},\"edgeFunction\":{}}");

            var mockHttpClient = new Mock<HTTPClient>(null, null, null);
            mockHttpClient
                .Setup(httpClient => httpClient.Settings())
                .ReturnsAsync(_settings);

            _plugin = new Mock<StubEventPlugin>
            {
                CallBase = true
            };

            var config = new Configuration(
                writeKey: "123",
                storageProvider: new DefaultStorageProvider("tests"),
                autoAddHightouchDestination: false,
                useSynchronizeDispatcher: true,
                httpClientProvider: new MockHttpClientProvider(mockHttpClient)
            );
            _analytics = new Analytics(config);
        }

        [Fact]
        public void TestTrack()
        {
            var expected = new JsonObject
            {
                ["foo"] = "bar"
            };
            string expectedEvent = "foo";
            var actual = new List<TrackEvent>();
            _plugin.Setup(o => o.Track(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Track(expectedEvent, expected);

            Assert.NotEmpty(actual);
            Assert.Equal(expected, actual[0].Properties);
            Assert.Equal(expectedEvent, actual[0].Event);
        }

        [Fact]
        public void TestTrackNoProperties()
        {
            string expectedEvent = "foo";
            var actual = new List<TrackEvent>();
            _plugin.Setup(o => o.Track(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Track(expectedEvent);

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Properties.Count == 0);
            Assert.Equal(expectedEvent, actual[0].Event);
        }

        [Fact]
        public void TestTrackT()
        {
            var expected = new FooBar();
            string expectedEvent = "foo";
            var actual = new List<TrackEvent>();
            _plugin.Setup(o => o.Track(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Track(expectedEvent, expected);

            Assert.NotEmpty(actual);
            Assert.Equal(expected.GetJsonObject(), actual[0].Properties);
            Assert.Equal(expectedEvent, actual[0].Event);
        }

        [Fact]
        public void TestTrackTNullProperties()
        {
            string expectedEvent = "foo";
            var actual = new List<TrackEvent>();
            _plugin.Setup(o => o.Track(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Track(expectedEvent, (FooBar)null);

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Properties.Count == 0);
            Assert.Equal(expectedEvent, actual[0].Event);
        }

        [Fact]
        public void TestTrackTNoProperties()
        {
            string expectedEvent = "foo";
            var actual = new List<TrackEvent>();
            _plugin.Setup(o => o.Track(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Track(expectedEvent);

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Properties.Count == 0);
            Assert.Equal(expectedEvent, actual[0].Event);
        }

        [Fact]
        public void TestIdentify()
        {
            var expected = new JsonObject
            {
                ["foo"] = "bar"
            };
            string expectedUserId = "newUserId";
            var actual = new List<IdentifyEvent>();
            _plugin.Setup(o => o.Identify(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Identify(expectedUserId, expected);

            string actualUserId = _analytics.UserId();

            Assert.NotEmpty(actual);
            Assert.Equal(expected, actual[0].Traits);
            Assert.Equal(expectedUserId, actualUserId);
        }

        [Fact]
        public void TestIdentifyNoTraits()
        {
            string expectedUserId = "newUserId";
            var actual = new List<IdentifyEvent>();
            _plugin.Setup(o => o.Identify(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Identify(expectedUserId);

            string actualUserId = _analytics.UserId();

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Traits.Count == 0);
            Assert.Equal(expectedUserId, actualUserId);
        }

        [Fact]
        public void TestIdentifyNoUserId()
        {
            var expected = new JsonObject
            {
                ["foo"] = "bar"
            };
            var actual = new List<IdentifyEvent>();
            _plugin.Setup(o => o.Identify(Capture.In(actual)));
            string expectedUserId = _analytics.UserId();

            _analytics.Add(_plugin.Object);
            _analytics.Identify(expected);

            string actualUserId = _analytics.UserId();

            Assert.NotEmpty(actual);
            Assert.Equal(expected, actual[0].Traits);
            Assert.Equal(expectedUserId, actualUserId);
        }

        [Fact]
        public void TestIdentifyNoUserIdNullTraits()
        {
            var actual = new List<IdentifyEvent>();
            _plugin.Setup(o => o.Identify(Capture.In(actual)));
            string expectedUserId = _analytics.UserId();

            _analytics.Add(_plugin.Object);
            _analytics.Identify((JsonObject)null);

            string actualUserId = _analytics.UserId();

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Traits.Count == 0);
            Assert.Equal(expectedUserId, actualUserId);
        }

        [Fact]
        public void TestIdentifyT()
        {
            var expected = new FooBar();
            string expectedUserId = "newUserId";
            var actual = new List<IdentifyEvent>();
            _plugin.Setup(o => o.Identify(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Identify(expectedUserId, expected);

            string actualUserId = _analytics.UserId();

            Assert.NotEmpty(actual);
            Assert.Equal(expected.GetJsonObject(), actual[0].Traits);
            Assert.Equal(expectedUserId, actualUserId);
        }

        [Fact]
        public void TestIdentifyTNullTraits()
        {
            string expectedUserId = "newUserId";
            var actual = new List<IdentifyEvent>();
            _plugin.Setup(o => o.Identify(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Identify(expectedUserId, (FooBar)null);

            string actualUserId = _analytics.UserId();

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Traits.Count == 0);
            Assert.Equal(expectedUserId, actualUserId);
        }

        [Fact]
        public void TestIdentifyTNoTraits()
        {
            string expectedUserId = "newUserId";
            var actual = new List<IdentifyEvent>();
            _plugin.Setup(o => o.Identify(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Identify(expectedUserId, (FooBar)null);
            string actualUserId = _analytics.UserId();

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Traits.Count == 0);
            Assert.Equal(expectedUserId, actualUserId);
        }

        [Fact]
        public void TestIdentifyTNoUserId()
        {
            var expected = new FooBar();
            var actual = new List<IdentifyEvent>();
            _plugin.Setup(o => o.Identify(Capture.In(actual)));
            string expectedUserId = _analytics.UserId();

            _analytics.Add(_plugin.Object);
            _analytics.Identify(expected);
            string actualUserId = _analytics.UserId();

            Assert.NotEmpty(actual);
            Assert.Equal(expected.GetJsonObject(), actual[0].Traits);
            Assert.Equal(expectedUserId, actualUserId);
        }

        [Fact]
        public void TestIdentifyTNoUserIdNullTraits()
        {
            var actual = new List<IdentifyEvent>();
            _plugin.Setup(o => o.Identify(Capture.In(actual)));
            string expectedUserId = _analytics.UserId();

            _analytics.Add(_plugin.Object);
            _analytics.Identify((FooBar)null);
            string actualUserId = _analytics.UserId();

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Traits.Count == 0);
            Assert.Equal(expectedUserId, actualUserId);
        }

        [Fact]
        public void TestIdentifyReload()
        {
            string expectedUserId = "newUserId";
            var actualIdentify = new List<IdentifyEvent>();
            var actualTrack = new List<TrackEvent>();
            _plugin.Setup(o => o.Identify(Capture.In(actualIdentify)));
            _plugin.Setup(o => o.Track(Capture.In(actualTrack)));

            _analytics.Add(_plugin.Object);
            _analytics.Identify(expectedUserId);

            _analytics.Identify(null, null);

            var userIdEmpty = UserInfo.DefaultState(_analytics.Storage);
            Assert.Null(userIdEmpty._userId);
        }

        [Fact]
        public void TestScreen()
        {
            var expected = new JsonObject
            {
                ["foo"] = "bar"
            };
            string expectedTitle = "foo";
            string expectedCategory = "bar";
            var actual = new List<ScreenEvent>();
            _plugin.Setup(o => o.Screen(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Screen(expectedTitle, expected, expectedCategory);

            Assert.NotEmpty(actual);
            Assert.Equal(expected, actual[0].Properties);
            Assert.Equal(expectedTitle, actual[0].Name);
            Assert.Equal(expectedCategory, actual[0].Category);
        }

        [Fact]
        public void TestScreenWithNulls()
        {
            var actual = new List<ScreenEvent>();
            _plugin.Setup(o => o.Screen(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Screen(null, null, (string)null);

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Properties.Count == 0);
            Assert.Null(actual[0].Name);
            Assert.Null(actual[0].Category);
        }

        [Fact]
        public void TestScreenT()
        {
            var expected = new FooBar();
            string expectedTitle = "foo";
            string expectedCategory = "bar";
            var actual = new List<ScreenEvent>();
            _plugin.Setup(o => o.Screen(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Screen(expectedTitle, expected, expectedCategory);

            Assert.NotEmpty(actual);
            Assert.Equal(expected.GetJsonObject(), actual[0].Properties);
            Assert.Equal(expectedTitle, actual[0].Name);
            Assert.Equal(expectedCategory, actual[0].Category);
        }

        [Fact]
        public void TestScreenTWithNulls()
        {
            var actual = new List<ScreenEvent>();
            _plugin.Setup(o => o.Screen(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Screen(null, (FooBar)null, null);

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Properties.Count == 0);
            Assert.Null(actual[0].Name);
            Assert.Null(actual[0].Category);
        }

        [Fact]
        public void TestPage()
        {
            var expected = new JsonObject
            {
                ["foo"] = "bar"
            };
            string expectedTitle = "foo";
            string expectedCategory = "bar";
            var actual = new List<PageEvent>();
            _plugin.Setup(o => o.Page(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Page(expectedTitle, expected, expectedCategory);

            Assert.NotEmpty(actual);
            Assert.Equal(expected, actual[0].Properties);
            Assert.Equal(expectedTitle, actual[0].Name);
            Assert.Equal(expectedCategory, actual[0].Category);
            Assert.Equal("page", actual[0].Type);
        }

        [Fact]
        public void TestPageWithNulls()
        {
            var actual = new List<PageEvent>();
            _plugin.Setup(o => o.Page(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Page(null, null, (string)null);

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Properties.Count == 0);
            Assert.Null(actual[0].Name);
            Assert.Null(actual[0].Category);
            Assert.Equal("page", actual[0].Type);
        }

        [Fact]
        public void TestPageT()
        {
            var expected = new FooBar();
            string expectedTitle = "foo";
            string expectedCategory = "bar";
            var actual = new List<PageEvent>();
            _plugin.Setup(o => o.Page(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Page(expectedTitle, expected, expectedCategory);

            Assert.NotEmpty(actual);
            Assert.Equal(expected.GetJsonObject(), actual[0].Properties);
            Assert.Equal(expectedTitle, actual[0].Name);
            Assert.Equal(expectedCategory, actual[0].Category);
            Assert.Equal("page", actual[0].Type);
        }

        [Fact]
        public void TestPageTWithNulls()
        {
            var actual = new List<PageEvent>();
            _plugin.Setup(o => o.Page(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Page(null, (FooBar)null, null);

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Properties.Count == 0);
            Assert.Null(actual[0].Name);
            Assert.Null(actual[0].Category);
            Assert.Equal("page", actual[0].Type);
        }

        [Fact]
        public void TestGroup()
        {
            var expected = new JsonObject
            {
                ["foo"] = "bar"
            };
            string expectedGroupId = "foo";
            var actual = new List<GroupEvent>();
            _plugin.Setup(o => o.Group(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Group(expectedGroupId, expected);

            Assert.NotEmpty(actual);
            Assert.Equal(expected, actual[0].Traits);
            Assert.Equal(expectedGroupId, actual[0].GroupId);
        }

        [Fact]
        public void TestGroupNoProperties()
        {
            string expectedGroupId = "foo";
            var actual = new List<GroupEvent>();
            _plugin.Setup(o => o.Group(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Group(expectedGroupId);

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Traits.Count == 0);
            Assert.Equal(expectedGroupId, actual[0].GroupId);
        }

        [Fact]
        public void TestGroupT()
        {
            var expected = new FooBar();
            string expectedGroupId = "foo";
            var actual = new List<GroupEvent>();
            _plugin.Setup(o => o.Group(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Group(expectedGroupId, expected);

            Assert.NotEmpty(actual);
            Assert.Equal(expected.GetJsonObject(), actual[0].Traits);
            Assert.Equal(expectedGroupId, actual[0].GroupId);
        }

        [Fact]
        public void TestGroupTNullProperties()
        {
            string expectedGroupId = "foo";
            var actual = new List<GroupEvent>();
            _plugin.Setup(o => o.Group(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Group(expectedGroupId, (FooBar)null);

            Assert.NotEmpty(actual);
            Assert.True(actual[0].Traits.Count == 0);
            Assert.Equal(expectedGroupId, actual[0].GroupId);
        }

        [Fact]
        public void TestAlias()
        {
            string expectedPrevious = "foo";
            string expected = "bar";
            var actual = new List<AliasEvent>();
            _plugin.Setup(o => o.Alias(Capture.In(actual)));

            _analytics.Add(_plugin.Object);
            _analytics.Identify(expectedPrevious);
            _analytics.Alias(expected);

            Assert.NotEmpty(actual);
            Assert.Equal(expectedPrevious, actual[0].PreviousId);
            Assert.Equal(expected, actual[0].UserId);
        }

        [Fact]
        public void TestTrackWithContextSchemaVersion()
        {
            var actual = new List<TrackEvent>();
            _plugin.Setup(o => o.Track(Capture.In(actual)));
            _analytics.Add(_plugin.Object);

            _analytics.Track("Signed Up", new JsonObject(), ProtocolContext("v1"));

            Assert.NotEmpty(actual);
            Assert.Equal("v1", SchemaVersion(actual[0]));
            Assert.True(actual[0].Context.ContainsKey("library"));

            // StartupQueue replays via Process(event); context must survive a second pass.
            _analytics.Process(actual[0]);
            Assert.Equal("v1", SchemaVersion(actual[0]));
        }

        [Fact]
        public void TestAliasOverlappingContextDoesNotSwap()
        {
            var actual = new List<AliasEvent>();
            _plugin.Setup(o => o.Alias(Capture.In(actual)));
            _analytics.Add(_plugin.Object);

            _analytics.Identify("prev");
            _analytics.Alias("id-v1", ProtocolContext("v1"));
            _analytics.Alias("id-v2", ProtocolContext("v2"));

            Assert.Equal(2, actual.Count);
            Assert.Equal("id-v1", actual[0].UserId);
            Assert.Equal("v1", SchemaVersion(actual[0]));
            Assert.Equal("id-v2", actual[1].UserId);
            Assert.Equal("v2", SchemaVersion(actual[1]));

            var first = new AliasEvent("processed-v1", "prev");
            var second = new AliasEvent("processed-v2", "prev");
            _analytics.Process(second, ProtocolContext("v2"));
            _analytics.Process(first, ProtocolContext("v1"));
            Assert.Equal("v2", SchemaVersion(second));
            Assert.Equal("v1", SchemaVersion(first));
        }

        [Fact]
        public void TestOldOverloadsUnchanged()
        {
            var tracks = new List<TrackEvent>();
            var aliases = new List<AliasEvent>();
            _plugin.Setup(o => o.Track(Capture.In(tracks)));
            _plugin.Setup(o => o.Alias(Capture.In(aliases)));
            _analytics.Add(_plugin.Object);

            var properties = new JsonObject { ["foo"] = "bar" };
            _analytics.Track("foo", properties);
            _analytics.Identify("prev");
            _analytics.Alias("next");

            Assert.NotEmpty(tracks);
            Assert.Equal(properties, tracks[0].Properties);
            Assert.Equal("foo", tracks[0].Event);
            Assert.False(tracks[0].Context.ContainsKey("protocols"));
            Assert.True(tracks[0].Context.ContainsKey("library"));

            Assert.NotEmpty(aliases);
            Assert.Equal("prev", aliases[0].PreviousId);
            Assert.Equal("next", aliases[0].UserId);
            Assert.False(aliases[0].Context.ContainsKey("protocols"));
        }

        [Fact]
        public void TestScreenIdentifyGroupAliasContext()
        {
            var screens = new List<ScreenEvent>();
            var pages = new List<PageEvent>();
            var identifies = new List<IdentifyEvent>();
            var groups = new List<GroupEvent>();
            var aliases = new List<AliasEvent>();
            _plugin.Setup(o => o.Screen(Capture.In(screens)));
            _plugin.Setup(o => o.Page(Capture.In(pages)));
            _plugin.Setup(o => o.Identify(Capture.In(identifies)));
            _plugin.Setup(o => o.Group(Capture.In(groups)));
            _plugin.Setup(o => o.Alias(Capture.In(aliases)));
            _analytics.Add(_plugin.Object);

            _analytics.Screen("Home", new JsonObject(), ProtocolContext("v1"));
            _analytics.Page("Home", new JsonObject(), ProtocolContext("v1"));
            _analytics.Identify("user", new JsonObject(), ProtocolContext("v1"));
            _analytics.Identify(new JsonObject(), ProtocolContext("v1"));
            _analytics.Group("grp", new JsonObject(), ProtocolContext("v1"));
            _analytics.Alias("alias-id", ProtocolContext("v2"));

            Assert.Equal("v1", SchemaVersion(screens[0]));
            Assert.Equal("v1", SchemaVersion(pages[0]));
            Assert.Equal("v1", SchemaVersion(identifies[0]));
            Assert.Equal("v1", SchemaVersion(identifies[1]));
            Assert.Equal("v1", SchemaVersion(groups[0]));
            Assert.Equal("v2", SchemaVersion(aliases[0]));
        }

        [Fact]
        public void TestTrackContextDeepMergesNestedMaps()
        {
            var actual = new List<TrackEvent>();
            _plugin.Setup(o => o.Track(Capture.In(actual)));
            _analytics.Add(_plugin.Object);

            var context = new Dictionary<string, object>
            {
                ["protocols"] = new Dictionary<string, object>
                {
                    ["schemaVersion"] = "v1"
                },
                ["locale"] = "en-US"
            };
            _analytics.Track("foo", new JsonObject(), context);

            Assert.Equal("v1", SchemaVersion(actual[0]));
            Assert.Equal("en-US", actual[0].Context.GetString("locale"));
            Assert.True(actual[0].Context.ContainsKey("library"));
        }

        private static IDictionary<string, object> ProtocolContext(string version)
        {
            return new Dictionary<string, object>
            {
                ["protocols"] = new Dictionary<string, object>
                {
                    ["schemaVersion"] = version
                }
            };
        }

        private static string SchemaVersion(RawEvent @event)
        {
            return @event.Context.GetJsonObject("protocols").GetString("schemaVersion");
        }
    }
}
