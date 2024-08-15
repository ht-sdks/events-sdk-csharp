// See https://aka.ms/new-console-template for more information

using System;
using ConsoleSample;
using Hightouch.Events;

var analytics = new Analytics(new Configuration("WRITE_KEY", apiHost: "http://localhost:7777", flushAt: 1));
Analytics.Logger = new HightouchLogger();

analytics.Identify("foo");
analytics.Track("track right after identify");

Console.ReadLine();
