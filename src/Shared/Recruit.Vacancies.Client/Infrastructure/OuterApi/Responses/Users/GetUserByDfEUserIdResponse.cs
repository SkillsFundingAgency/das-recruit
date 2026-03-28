using Esfa.Recruit.Vacancies.Client.Infrastructure.User;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Users;

public sealed record GetUserByDfEUserIdResponse
{
    public UserDto User { get; init; }
}
