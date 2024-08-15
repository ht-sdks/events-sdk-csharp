// See https://aka.ms/new-console-template for more information

using System;
using ConsoleSample;
using Hightouch.Events;

var analytics = new Analytics(new Configuration("67de78e61b519fc4678541119d426e927a77572331ad3f86db0059c07aaa4335", apiHost: "http://localhost:7777", flushAt: 1));
Analytics.Logger = new HightouchLogger();

analytics.Identify("foo");
analytics.Track("track right after identify");

Console.ReadLine();
