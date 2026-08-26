using System.Collections.Generic;
using Hightouch.Events.Serialization;
using Xunit;

namespace Tests.Serialization
{
    public class JsonMergeTest
    {
        [Fact]
        public void DeepMergeMergesNestedMaps()
        {
            var target = new JsonObject
            {
                ["protocols"] = new JsonObject
                {
                    ["keep"] = "yes"
                },
                ["locale"] = "en"
            };

            JsonMerge.DeepMerge(target, new Dictionary<string, object>
            {
                ["protocols"] = new Dictionary<string, object>
                {
                    ["schemaVersion"] = "v1"
                },
                ["ip"] = "1.1.1.1"
            });

            Assert.Equal("yes", target.GetJsonObject("protocols").GetString("keep"));
            Assert.Equal("v1", target.GetJsonObject("protocols").GetString("schemaVersion"));
            Assert.Equal("en", target.GetString("locale"));
            Assert.Equal("1.1.1.1", target.GetString("ip"));
        }

        [Fact]
        public void DeepMergeNullSourceIsNoOp()
        {
            var target = new JsonObject { ["foo"] = "bar" };
            JsonMerge.DeepMerge(target, (IDictionary<string, object>)null);
            Assert.Equal("bar", target.GetString("foo"));
            Assert.Equal(1, target.Count);
        }
    }
}
