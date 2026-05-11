using Esfa.Recruit.Vacancies.Client.Infrastructure.User;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Users;

public sealed record GetUserByIdamsUserIdResponse
{
    public UserDto User { get; init; }
}
