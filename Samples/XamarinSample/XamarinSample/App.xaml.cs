﻿using System;
using Hightouch.Events;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace XamarinSample
{
    public partial class App : Application
    {
        public static Analytics analytics { get; private set; }

        public App()
        {
            InitializeComponent();

            var configuration = new Configuration("WRITE_KEY", flushAt: 1);
            analytics = new Analytics(configuration);

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            analytics.Track("Application Opened");
        }

        protected override void OnSleep()
        {
            analytics.Track("Application Backgrounded");
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
