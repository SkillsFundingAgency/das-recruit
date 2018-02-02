using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Employer.Web.Controllers
{
  public class ContentPolicyReportController : Controller
  {

    ILogger<ContentPolicyReportController> _logger;

    public ContentPolicyReportController(ILogger<ContentPolicyReportController> logger)
    {
      _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Report([FromBody] CspReportRequest request)
    {
        _logger.LogWarning("CSP Violation: {cspReport}", request);

        return Ok();
    } 

    public class CspReportRequest
    {
        [JsonProperty(PropertyName = "csp-report")]
        public CspReport CspReport { get; set; }
    }

    public class CspReport
    {
        [JsonProperty(PropertyName = "document-uri")]
        public string DocumentUri { get; set; }

        [JsonProperty(PropertyName = "referrer")]
        public string Referrer { get; set; }

        [JsonProperty(PropertyName = "violated-directive")]
        public string ViolatedDirective { get; set; }

        [JsonProperty(PropertyName = "effective-directive")]
        public string EffectiveDirective { get; set; }

        [JsonProperty(PropertyName = "original-policy")]
        public string OriginalPolicy { get; set; }

        [JsonProperty(PropertyName = "blocked-uri")]
        public string BlockedUri { get; set; }

        [JsonProperty(PropertyName = "status-code")]
        public int StatusCode { get; set; }
    }
  }
}