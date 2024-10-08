using System;
using Hightouch.Events;
using Hightouch.Events.Concurrent;
using UnityEngine;

namespace UnitySample
{
    public class SingletonAnalytics : Singleton<SingletonAnalytics>
    {
        public Analytics Analytics { get; set; }

        protected override void Awake()
        {
            // you don't have to use `UnityHTTPClientProvider`
            // the default httpClientProvider works on Unity, too.
            Configuration configuration =
                new Configuration("WRITE_KEY",
                    exceptionHandler: new ErrorHandler(),
                    httpClientProvider: new UnityHTTPClientProvider(MainThreadDispatcher.Instance));
            Analytics = new Analytics(configuration);
            Analytics.Add(new LifecyclePlugin());
        }

        class ErrorHandler : ICoroutineExceptionHandler
        {
            public void OnExceptionThrown(Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
