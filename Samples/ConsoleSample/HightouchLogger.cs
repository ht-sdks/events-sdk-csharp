using System;
using Hightouch.Events.Utilities;

namespace ConsoleSample
{
    class HightouchLogger : IHightouchLogger
    {
        public void Log(LogLevel logLevel, Exception exception = null, string message = null)
        {
            switch (logLevel)
            {
                case LogLevel.Warning:
                case LogLevel.Information:
                case LogLevel.Debug:
                    Console.Out.WriteLine("Message: " + message);
                    break;
                case LogLevel.Critical:
                case LogLevel.Trace:
                case LogLevel.Error:
                    Console.Error.WriteLine("Exception: " + exception?.StackTrace);
                    Console.Error.WriteLine("Message: " + message);
                    break;
                case LogLevel.None:
                default:
                    break;
            }
        }
    }
}
