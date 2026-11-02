using System;
using System.Diagnostics;
using AngleSharp;
using AngleSharp.Dom;
using Ganss.Xss;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Services;

public class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly ILogger<HtmlSanitizerService> _logger;

    public HtmlSanitizerService(ILogger<HtmlSanitizerService> logger)
    {
        _logger = logger;
    }

    public string Sanitize(string html)
    {
        return string.IsNullOrWhiteSpace(html) ? 
            html : 
            Sanitize(html, null);
    }

    public bool IsValid(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return true;

        var isValid = true;

        var sanitized = Sanitize(html, () => isValid = false);

        if (string.IsNullOrWhiteSpace(sanitized))
            return false;

        return isValid;
    }

    private string Sanitize(string html, Action removingHtmlCallback)
    {
        var watch = Stopwatch.StartNew();

        string sanitized;
        var sanitizer = GetSanitizer(removingHtmlCallback);
        using (var dom = sanitizer.SanitizeDom(html))
        {
            sanitized = string.IsNullOrWhiteSpace(dom.Body.GetInnerText()) ? 
                string.Empty :
                dom.Body.ChildNodes.ToHtml(HtmlSanitizer.DefaultOutputFormatter);
        }
        watch.Stop();

        _logger.LogInformation("Sanitized html input in {timer}ms", watch.ElapsedMilliseconds);

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