using System.Collections.Generic;

namespace Hightouch.Events.Utilities
{
    public interface IEventPipelineProvider
    {
        IEventPipeline Create(Analytics analytics, string key);
    }
}
