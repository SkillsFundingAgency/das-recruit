using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.User.Requests;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.User;

public interface IUserRepositoryRunner
{
    Task UpsertUserAsync(Domain.Entities.User user);
}

public class UserRepositoryRunner : IUserRepositoryRunner
{
    private readonly IEnumerable<IUserWriteRepository> _vacancyReviewResolver;

    public UserRepositoryRunner(IEnumerable<IUserWriteRepository> vacancyReviewResolver)
    {
        _vacancyReviewResolver = vacancyReviewResolver;
    }

    public async Task UpsertUserAsync(Domain.Entities.User user)
    {
        foreach (var vacancyReviewResolver in _vacancyReviewResolver)
        {
            await vacancyReviewResolver.UpsertUserAsync(user);
        }
    }
}

public class UserService(IOuterApiClient outerApiClient) : IUserWriteRepository
{
    public async Task UpsertUserAsync(Domain.Entities.User user)
    {
        var request = new PostUserRequest(user.Id, (UserDto)user);

        await outerApiClient.Post(request, false);
    }
}