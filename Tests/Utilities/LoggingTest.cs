using System;
using Moq;
using Hightouch.Events;
using Hightouch.Events.Utilities;
using Hightouch.Events.Serialization;
using Hightouch.Events.Sovran;
using Xunit;

namespace Tests.Utilities
{
    public class LoggingTest
    {

        private readonly Store _store;

        private Settings _settings;

        private readonly Configuration _configuration;

        private readonly Mock<IStorage> _storage;

        public LoggingTest()
        {
            _store = new Store(true);
            _settings = new Settings
            {
                Integrations = new JsonObject
                {
                    ["foo"] = "bar"
                }
            };
            _configuration = new Configuration(
                writeKey: "123",
                autoAddHightouchDestination: false,
                useSynchronizeDispatcher: true,
                defaultSettings: _settings
            );
            _storage = new Mock<IStorage>();
        }

        [Fact]
        public void TestLog()
        {
            var logger = new Mock<IHightouchLogger>();
            Analytics.Logger = logger.Object;
            var exception = new Exception();
            _storage
                .Setup(o => o.Read(It.IsAny<StorageConstants>()))
                .Throws(exception);

            Hightouch.Events.System.DefaultState(_configuration, _storage.Object);

            logger.Verify(o => o.Log(LogLevel.Error, exception, It.IsAny<string>()), Times.Exactly(1));
        }
    }
}
