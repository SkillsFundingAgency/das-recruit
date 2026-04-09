using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Users;
using Esfa.Recruit.Vacancies.Client.Infrastructure.User.Requests;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.User;

public class UserService(IOuterApiClient outerApiClient, IEncodingService encodingService) :
    IUserRepository, IUserWriteRepository
{
    public async Task UpsertUserAsync(Domain.Entities.User user)
    {
        var request = new PostUserRequest(user.Id, UserDto.From(user, encodingService));

        await outerApiClient.Post(request, false);
    }

    public async Task<Domain.Entities.User> GetAsync(string idamsUserId)
    {
        var request = new GetUserByIdamsUserIdRequest(idamsUserId);

        var response = await outerApiClient.Get<GetUserByIdamsUserIdResponse>(request);

        return response.User == null ? null : MapUser(response.User);
    }

    public async Task<Domain.Entities.User> GetByDfEUserId(string dfEUserId)
    {
        var request = new GetUserByDfEUserIdRequest(dfEUserId);

        var response = await outerApiClient.Get<GetUserByDfEUserIdResponse>(request);

        return response.User == null ? null : MapUser(response.User);
    }

    public async Task<List<Domain.Entities.User>> GetEmployerUsersAsync(string accountId)
    {
        var employerAccountId = encodingService.Decode(accountId, EncodingType.AccountId);

        var request = new GetUserByEmployerAccountIdRequest(employerAccountId);

        var response = await outerApiClient.Get<GetUserByEmployerAccountIdResponse>(request);

        return response.Users == null || response.Users.Count == 0 
            ? [] 
            : response.Users.Select(MapUser).ToList();
    }

    public async Task<List<Domain.Entities.User>> GetProviderUsersAsync(long ukprn)
    {
        var request = new GetUserByProviderUkprnRequest(ukprn);

        var response = await outerApiClient.Get<GetUserByProviderUkprnResponse>(request);

        return response.Users == null || response.Users.Count == 0
            ? []
            : response.Users.Select(MapUser).ToList();
    }

    public async Task<Domain.Entities.User> GetUserByEmail(string email, UserType userType)
    {
        var request = new GetUserByEmailRequest(email, userType);

        var response = await outerApiClient.Post<GetUserByEmailResponse>(request);

        return response.User == null ? null : MapUser(response.User);
    }
    
    private static Domain.Entities.User MapUser(UserDto source) =>
        new()
        {
            Id = source.Id,
            Name = source.Name,
            IdamsUserId = source.IdamsUserId,
            UserType = source.UserType,
            Email = source.Email,
            CreatedDate = source.CreatedDate,
            LastSignedInDate = source.LastSignedInDate,
            Ukprn = source.Ukprn,
            EmployerAccountIds = source.EmployerAccountIds.Select(x => x.ToString()).ToList(),
            DfEUserId = source.DfEUserId,
            ClosedVacanciesBlockedProviderAlertDismissedOn = source.ClosedVacanciesBlockedProviderAlertDismissedOn,
            TransferredVacanciesBlockedProviderAlertDismissedOn = source.TransferredVacanciesBlockedProviderAlertDismissedOn,
            ClosedVacanciesWithdrawnByQaAlertDismissedOn = source.ClosedVacanciesWithdrawnByQaAlertDismissedOn,
            TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn = source.TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn
        };
}