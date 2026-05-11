using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.User.Requests;

public sealed record GetUserByIdamsUserIdRequest(string IdamsUserId) : IGetApiRequest
{
    public string GetUrl => $"users/by/idams/{IdamsUserId}";
}