using System.Collections.Generic;
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

        foreach (var cssClass in GetModifierClasses(Status))
        {
            output.AddClass(cssClass, HtmlEncoder.Default);
        }
    }

    private static IEnumerable<string> GetModifierClasses(VacancyStatus? status) => status switch
    {
        VacancyStatus.Draft => ["govuk-tag--grey"],
        VacancyStatus.Review => ["govuk-tag", "govuk-tag--blue"],    // pending employer review
        VacancyStatus.Submitted => ["govuk-tag", "govuk-tag--blue"], // pending DfE review / ready for review
        VacancyStatus.Referred => ["govuk-tag--red"],                           // rejected by employer
        VacancyStatus.Rejected => ["govuk-tag--red"],
        VacancyStatus.Live => ["govuk-tag--turquoise"],
        VacancyStatus.Closed => ["govuk-tag--grey"],
        VacancyStatus.Approved => ["govuk-tag--green"],
        VacancyStatus.Archived => ["govuk-tag--grey"],
        _ => []
    };

    protected override Task<IHtmlContent> GetContent(TagHelperOutput output)
    {
        if (!Status.HasValue) return base.GetContent(output);

        var display = Status.GetDisplayName(UserType);
        return Task.FromResult<IHtmlContent>(new HtmlString(display));
    } 
}