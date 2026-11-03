using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Provider.Web.Controllers;

public class ContentPolicyReportController(ILogger<ContentPolicyReportController> logger) : Controller
{
    [AllowAnonymous]
    [HttpPost("contentPolicyReport/report")]
    [IgnoreAntiforgeryToken]
    public IActionResult Report([FromBody] CspReportRequest request)
    {
        logger.LogInformation("Request Path: {Path} and Query: {Query}", HttpContext.Request.Path, HttpContext.Request.Query);
            
        logger.LogWarning("CSP Violation: {cspReport}", request);

        return Ok();
    }

    public class CspReportRequest
    {
        [JsonProperty(PropertyName = "csp-report")]
        public CspReport CspReport { get; set; }

        public override string ToString()
        {
            return $"Violated: {CspReport.ViolatedDirective} by {CspReport.BlockedUri}";
        }
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