using Microsoft.AspNetCore.Mvc.RazorPages;
using Hightouch.Events;

namespace AspNetSample
{
    public class AnalyticsPageModel : PageModel
    {
        public readonly Analytics analytics;

        public AnalyticsPageModel(Analytics analytics) => this.analytics = analytics;
    }
}
