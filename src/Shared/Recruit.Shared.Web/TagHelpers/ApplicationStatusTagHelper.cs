using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(ApplicationStatusTagHelper.TagName)]
public class ApplicationStatusTagHelper : RaaTagsTagHelper
{
    public new const string TagName = "govuk-tag-application-review-status";
    public ApplicationReviewStatus? ApplicationStatus { get; set; }
    public UserType? userType { get; set; }
    public bool IsWithdrawn { get; set; } = false;

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
        if (IsWithdrawn)
        {
            return "govuk-tag--grey";
        }

        if (ApplicationStatus.HasValue)
        {
            return ApplicationStatus switch
            {
                ApplicationReviewStatus.New => "govuk-tag--light-blue",
                ApplicationReviewStatus.InReview => "govuk-tag--yellow",
                ApplicationReviewStatus.Unsuccessful => "govuk-tag--orange", // "Unsuccessful"
                ApplicationReviewStatus.EmployerUnsuccessful => 
                    userType == UserType.Employer 
                        ? "govuk-tag--orange" // "Unsuccessful"
                        : "govuk-tag--pink", // "Employer reviewed"
                ApplicationReviewStatus.Shared => 
                    userType == UserType.Employer 
                        ? "govuk-tag--blue" // "Response needed"
                        : "govuk-tag--yellow", // "Shared"
                ApplicationReviewStatus.Interviewing => "govuk-tag--purple", // "Interviewing"
                ApplicationReviewStatus.EmployerInterviewing => 
                    userType == UserType.Employer 
                        ? "govuk-tag--purple" // "Interviewing"
                        : "govuk-tag--pink", // "Employer reviewed"
                ApplicationReviewStatus.Successful => "govuk-tag--green",
                _ => string.Empty
            };
        }
        return string.Empty;
    }

    protected override Task<IHtmlContent> GetContent(TagHelperOutput output)
    {
        if (IsWithdrawn)
        {
            return Task.FromResult<IHtmlContent>(new HtmlString("Withdrawn"));
        }

        if (ApplicationStatus.HasValue)
        {
            var display = ApplicationStatus.GetDisplayName(userType) ?? string.Empty;
            return Task.FromResult<IHtmlContent>(new HtmlString(display));
        }

        return base.GetContent(output);
    }
}