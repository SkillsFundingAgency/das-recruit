using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.User;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Users;

public sealed record GetUserByEmployerAccountIdResponse
{
    public List<UserDto> Users { get; init; }
}
