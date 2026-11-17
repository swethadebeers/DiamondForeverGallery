using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace DiamondForeverGallery.Pages
{
    // Tells browsers and proxies never to cache the error page.
    [ResponseCache(
    // Lets the error page work even if the security token check fails.
    [IgnoreAntiforgeryToken]
    /// <summary>
    /// Code behind the Error page shown when something goes wrong. It shows a request ID to help track the problem.
    /// </summary>
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public void OnGet()
        {
            // Use the trace ID if available; otherwise fall back to the request's own identifier.
            RequestId = Activity.Current
        }
    }

}
