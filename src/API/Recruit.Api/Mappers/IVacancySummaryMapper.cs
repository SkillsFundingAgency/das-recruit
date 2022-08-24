using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Services;

namespace SFA.DAS.Recruit.Api.Mappers
{
    public interface IVacancySummaryMapper
    {
        VacancySummary MapFromVacancySummaryProjection(VacancySummaryProjection vsp, bool isForProviderOwnedVacancies);
    }
}