using System.Diagnostics;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
using Ganss.XSS;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class HtmlSanitizerService : IHtmlSanitizerService
    {
        private readonly HtmlSanitizer _sanitizer;
        private readonly ILogger<HtmlSanitizerService> _logger;

        public HtmlSanitizerService(ILogger<HtmlSanitizerService> logger)
        {
            _logger = logger;

            _sanitizer = new HtmlSanitizer();
            _sanitizer.AllowedSchemes.Clear();
            _sanitizer.AllowDataAttributes = false;
            _sanitizer.AllowedAtRules.Clear();
            _sanitizer.AllowedCssProperties.Clear();
            _sanitizer.AllowedAttributes.Clear();

            _sanitizer.AllowedTags.Clear();
            _sanitizer.AllowedTags.Add("p");
            _sanitizer.AllowedTags.Add("br");
            _sanitizer.AllowedTags.Add("ul");
            _sanitizer.AllowedTags.Add("li");
        }

        public string Sanitize(string html)
        {
            if (html == null)
                return null;

            html = html.Replace("\r\n", "\n");

            var watch = Stopwatch.StartNew();
            var sanitized = _sanitizer.Sanitize(html);
            watch.Stop();

            if (sanitized.Equals(html))
            {
                _logger.LogInformation("Sanitized html input with no changes in {timer}ms", watch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation("Sanitized html input from: \"{unsanitized}\" to: \"{sanitized}\" in {timer}ms", html, sanitized, watch.ElapsedMilliseconds);                
            }

            return sanitized;
        }
    }
}
