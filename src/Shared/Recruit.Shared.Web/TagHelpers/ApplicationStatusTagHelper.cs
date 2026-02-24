using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName)]
public class ApplicationStatusTagHelper : RaaTagsTagHelper
{
    public new const string TagName = "govuk-tag-application-review-status";
    public ApplicationReviewStatus? ApplicationStatus { get; set; }
    public UserType? UserType { get; set; }

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
        if (ApplicationStatus.HasValue)
        {
            return ApplicationStatus switch
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
        if (ApplicationStatus.HasValue)
        {
            var display = ApplicationStatus.GetDisplayName(UserType) ?? string.Empty;
            return Task.FromResult<IHtmlContent>(new HtmlString(display));
        }

        return base.GetContent(output);
    }
}