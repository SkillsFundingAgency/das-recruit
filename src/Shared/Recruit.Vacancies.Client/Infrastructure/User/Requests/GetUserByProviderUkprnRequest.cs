using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.User.Requests;

public sealed record GetUserByProviderUkprnRequest(long Ukprn) : IGetApiRequest
{
    public string GetUrl => $"users/by/ukprn/{Ukprn}";
}