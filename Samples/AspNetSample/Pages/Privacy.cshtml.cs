﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Hightouch.Events;

namespace AspNetSample.Pages
{
    public class PrivacyModel : AnalyticsPageModel
    {
        public PrivacyModel(Analytics analytics) : base(analytics)
        {
        }


        public void OnGet()
        {
        }
    }
}
