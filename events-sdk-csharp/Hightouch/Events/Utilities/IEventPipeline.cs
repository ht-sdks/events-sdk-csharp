namespace Hightouch.Events.Utilities
{
    public interface IEventPipeline
    {
        bool Running { get; }
        string ApiHost { get; set; }

        void Put(RawEvent @event);
        void Flush();
        void Start();
        void Stop();
    }
}
