using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

[HtmlTargetElement(TagName)]
public class VacancyStatusTagHelper: RaaTagsTagHelper
{
    public new const string TagName = "govuk-tag-vacancy-status";
    public VacancyStatus? Status { get; set; }
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
        if (Status.HasValue)
        {
            return Status switch
            {
                VacancyStatus.Draft => "govuk-tag--grey",
                VacancyStatus.Review => "govuk-tag--blue", // pending employer review
                VacancyStatus.Submitted => "govuk-tag--blue", // pending DfE review / ready for review
                VacancyStatus.Referred => "govuk-tag--red", // rejected by employer
                VacancyStatus.Rejected => "govuk-tag--red",
                VacancyStatus.Live => "govuk-tag--turquoise",
                VacancyStatus.Closed => "govuk-tag--grey",
                VacancyStatus.Approved => "govuk-tag--green",
                _ => string.Empty
            };
        }
        return string.Empty;
    }

    protected override Task<IHtmlContent> GetContent(TagHelperOutput output)
    {
        if (Status.HasValue)
        {
            var display = Status.GetDisplayName(UserType);
            return Task.FromResult<IHtmlContent>(new HtmlString(display));
        }

        return base.GetContent(output);
    } 
}

