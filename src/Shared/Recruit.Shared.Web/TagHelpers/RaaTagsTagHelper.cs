using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName)]
[OutputElementHint("strong")]
public class RaaTagsTagHelper : TagHelper
{
    public const string TagName = "govuk-tag";

    public string Class { get; set; }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "strong";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.AddClass("govuk-tag", HtmlEncoder.Default);
        if (!string.IsNullOrWhiteSpace(Class))
        {
            var classes = Class.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (string className in classes)
            {
                output.AddClass(className, HtmlEncoder.Default);
            }
        }

        var content = await GetContent(output);
        output.Content.AppendHtml(content);
    }

    protected virtual async Task<IHtmlContent> GetContent(TagHelperOutput output)
    {
        return await output.GetChildContentAsync();
    }
}

[HtmlTargetElement(TagName)]
public class FoundationTagTagHelper : RaaTagsTagHelper
{
    public new const string TagName = "govuk-tag-foundation";
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.AddClass("govuk-tag--pink", HtmlEncoder.Default);
        await base.ProcessAsync(context, output);
    }

    protected override Task<IHtmlContent> GetContent(TagHelperOutput output)
    {
        return Task.FromResult<IHtmlContent>(new HtmlString("Foundation"));
    }
}

[HtmlTargetElement(TagName)]
public class ApiSubmittedTagHelper : RaaTagsTagHelper
{
    public new const string TagName = "govuk-tag-api-submitted";
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.AddClass("govuk-tag--yellow", HtmlEncoder.Default);
        await base.ProcessAsync(context, output);
    }

    protected override Task<IHtmlContent> GetContent(TagHelperOutput output)
    {
        return Task.FromResult<IHtmlContent>(new HtmlString("API submitted"));
    }
}

[HtmlTargetElement("govuk-status-tag")]
public class StatusTagHelper : RaaTagsTagHelper
{
    public VacancyStatus? VacancyStatusValue { get; set; }
    public ApplicationReviewStatus? ApplicationStatusValue { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await base.ProcessAsync(context, output);

        var modifier = GetModifierClass();

        if (!string.IsNullOrEmpty(modifier))
        {
            output.AddClass(modifier, HtmlEncoder.Default);
        }
    }

    private string GetModifierClass()
    {
        if (VacancyStatusValue.HasValue)
        {
            return VacancyStatusValue switch
            {
                VacancyStatus.Draft => "govuk-tag--grey",
                VacancyStatus.Review => "govuk-tag--blue",     // pending employer review
                VacancyStatus.Submitted => "govuk-tag--blue",  // pending DfE review / ready for review
                VacancyStatus.Referred => "govuk-tag--red",    // rejected by employer
                VacancyStatus.Rejected => "govuk-tag--red",
                VacancyStatus.Live => "govuk-tag--turquoise",
                VacancyStatus.Closed => "govuk-tag--grey",
                VacancyStatus.Approved => "govuk-tag--green",
                _ => string.Empty
            };
        }

        if (ApplicationStatusValue.HasValue)
        {
            return ApplicationStatusValue.Value switch
            {
                ApplicationReviewStatus.New => "govuk-tag--light-blue",
                ApplicationReviewStatus.InReview => "govuk-tag--yellow",
                ApplicationReviewStatus.Unsuccessful => "govuk-tag--orange",
                ApplicationReviewStatus.EmployerUnsuccessful => "govuk-tag--orange",
                ApplicationReviewStatus.Shared => "govuk-tag--yellow",
                ApplicationReviewStatus.Interviewing => "govuk-tag--purple",
                ApplicationReviewStatus.EmployerInterviewing => "govuk-tag--pink",
                ApplicationReviewStatus.Successful => "govuk-tag--green",
                _ => string.Empty
            };
        }

        return string.Empty;
    }

    protected override Task<IHtmlContent> GetContent(TagHelperOutput output)
    {
        if (VacancyStatusValue.HasValue)
        {
            return Task.FromResult<IHtmlContent>(new HtmlString(VacancyStatusValue.Value.ToString()));
        }

        if (ApplicationStatusValue.HasValue)
        {
            return Task.FromResult<IHtmlContent>(new HtmlString(ApplicationStatusValue.Value.ToString()));
        }

        return base.GetContent(output);
    }
}