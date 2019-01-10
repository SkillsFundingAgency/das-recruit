using System;
using System.Diagnostics;
using Ganss.XSS;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class HtmlSanitizerService : IHtmlSanitizerService
    {
        private readonly ILogger<HtmlSanitizerService> _logger;

        public HtmlSanitizerService(ILogger<HtmlSanitizerService> logger)
        {
            _logger = logger;
        }

        public string Sanitize(string html)
        {
            return Sanitize(html, null);
        }

        public bool IsValid(string html)
        {
            var isValid = true;

            Sanitize(html, () => isValid = false);

            return isValid;
        }

        private string Sanitize(string html, Action removingHtmlCallback)
        {
            if (html == null)
                return null;

            var sanitizer = GetSanitizer(removingHtmlCallback);

            var watch = Stopwatch.StartNew();
            var sanitized = sanitizer.Sanitize(html);
            watch.Stop();

            if (sanitized == html)
            {
                _logger.LogInformation("Sanitized html input with no changes in {timer}ms", watch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation("Sanitized html input from: \"{unsanitized}\" to: \"{sanitized}\" in {timer}ms", html, sanitized, watch.ElapsedMilliseconds);                
            }

            return sanitized;
        }

        private HtmlSanitizer GetSanitizer(Action removingHtmlCallback = null)
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedSchemes.Clear();
            sanitizer.AllowDataAttributes = false;
            sanitizer.AllowedAtRules.Clear();
            sanitizer.AllowedCssProperties.Clear();
            sanitizer.AllowedAttributes.Clear();

            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedTags.Add("p");
            sanitizer.AllowedTags.Add("br");
            sanitizer.AllowedTags.Add("ul");
            sanitizer.AllowedTags.Add("li");

            if (removingHtmlCallback != null)
            {
                sanitizer.RemovingAtRule += (s, e) => removingHtmlCallback();
                sanitizer.RemovingAttribute += (s, e) => removingHtmlCallback();
                sanitizer.RemovingComment += (s, e) => removingHtmlCallback();
                sanitizer.RemovingCssClass += (s, e) => removingHtmlCallback();
                sanitizer.RemovingStyle += (s, e) => removingHtmlCallback();
                sanitizer.RemovingTag += (s, e) => removingHtmlCallback();
            }
            
            return sanitizer;
        }
    }
}
