using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA
{
    public interface IFaaService
    {
        Task PublishApplicationStatusSummaryAsync(FaaApplicationStatusSummary message);
        Task PublishVacancyStatusSummaryAsync(FaaVacancyStatusSummary message);
    }
}