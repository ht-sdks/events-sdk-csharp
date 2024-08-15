// See https://aka.ms/new-console-template for more information

using System;
using ConsoleSample;
using Hightouch.Events;
using Hightouch.Events.Utilities;

var analytics = new Analytics(new Configuration("WRITE_KEY",
    apiHost: "http://localhost:7777",
    flushAt: 1,
    storageProvider: new InMemoryStorageProvider()));
Analytics.Logger = new HightouchLogger();

analytics.Identify("foo");
analytics.Track("track right after identify");

Console.ReadLine();
