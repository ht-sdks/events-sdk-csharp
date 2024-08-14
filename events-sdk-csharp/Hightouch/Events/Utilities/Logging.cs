using System;

namespace Hightouch.Events.Utilities
{
    public interface IHightouchLogger
    {
        void Log(LogLevel logLevel, Exception exception = null, string message = null);
    }

    public enum LogLevel
    {
        Trace, Debug, Information, Warning, Error, Critical, None
    }

    internal class StubLogger : IHightouchLogger
    {
        public void Log(LogLevel logLevel, Exception exception = null, string message = null) {}
    }
}
