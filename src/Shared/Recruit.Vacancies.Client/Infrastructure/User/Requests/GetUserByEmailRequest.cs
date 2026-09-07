using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.User.Requests;

public sealed record GetUserByEmailRequest(string Email, UserType UserType) : IPostApiRequest
{
    public string PostUrl => "users/by/email";
    public object Data { get; set; } = new
    {
        Email,
        UserType
    };
}
