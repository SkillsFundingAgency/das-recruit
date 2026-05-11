using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.User.Requests;

public sealed record GetUserByDfEUserIdRequest(string DfEUserId) : IGetApiRequest
{
    public string GetUrl => $"users/by/dfEUserId/{DfEUserId}";
}