using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Extensions;
public static class VacancyStatusExtensions
{
    public static string GetCssClassForVacancyStatus(this VacancyStatus status)
    {
        switch (status)
        {
            case VacancyStatus.Draft:
                return "govuk-tag govuk-tag--grey";
            case VacancyStatus.Review:
                // Pending employer review -> blue
                return "govuk-tag govuk-tag--blue";
            case VacancyStatus.Submitted:
                // Pending DfE review / Ready for review -> blue
                return "govuk-tag govuk-tag--blue";
            case VacancyStatus.Rejected:
                // Rejected by employer or DfE -> red
                return "govuk-tag govuk-tag--red";
            case VacancyStatus.Referred:
                // treated as rejected -> red
                return "govuk-tag govuk-tag--red";
            case VacancyStatus.Live:
                return "govuk-tag govuk-tag--turquoise";
            case VacancyStatus.Closed:
                return "govuk-tag govuk-tag--grey";
            case VacancyStatus.Approved:
                return "govuk-tag govuk-tag--green";
            default:
                return "govuk-tag";
        }
    }
}
