using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.User.Requests;

public sealed record GetUserByEmployerAccountIdRequest(long EmployerAccountId) : IGetApiRequest
{
    public string GetUrl => $"users/by/employerAccountId/{EmployerAccountId}";
}