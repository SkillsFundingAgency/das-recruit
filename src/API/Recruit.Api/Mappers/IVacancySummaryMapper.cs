using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Services;

namespace SFA.DAS.Recruit.Api.Mappers
{
    public interface IVacancySummaryMapper
    {
        VacancySummary MapFromVacancySummaryProjection(Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancySummary vsp, bool isForProviderOwnedVacancies);
    }
}