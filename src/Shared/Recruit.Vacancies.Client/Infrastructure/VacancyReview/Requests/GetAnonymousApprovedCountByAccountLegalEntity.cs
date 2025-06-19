using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;

public class GetAnonymousApprovedCountByAccountLegalEntity(long accountLegalEntityId) : IGetApiRequest
{
    public string GetUrl => $"accounts/{accountLegalEntityId}/vacancyreviews";
}