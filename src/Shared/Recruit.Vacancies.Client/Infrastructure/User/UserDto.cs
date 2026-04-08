using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Encoding;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.User;

public class UserDto
{
    public Guid Id { get; set; }
    public string? IdamsUserId { get; set; } 
    public required UserType UserType { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastSignedInDate { get; set; }
    public IList<long> EmployerAccountIds { get; set; } = [];
    public long? Ukprn { get; set; }
    public DateTime? TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn { get; set; }
    public DateTime? ClosedVacanciesBlockedProviderAlertDismissedOn { get; set; }
    public DateTime? TransferredVacanciesBlockedProviderAlertDismissedOn { get; set; }
    public DateTime? ClosedVacanciesWithdrawnByQaAlertDismissedOn { get; set; }
    public string? DfEUserId { get; set; }

    public static UserDto From(Domain.Entities.User source, IEncodingService encodingService)
    {
        var employerAccountIds = source.EmployerAccountIds
            .Select(x => encodingService.Decode(x, EncodingType.AccountId)).ToList();
        return new UserDto
        {
            Id = source.Id,
            Name = source.Name,
            IdamsUserId = source.IdamsUserId,
            UserType = source.UserType,
            Email = source.Email,
            CreatedDate = source.CreatedDate,
            LastSignedInDate = source.LastSignedInDate,
            Ukprn = source.Ukprn,
            EmployerAccountIds = employerAccountIds,
            DfEUserId = source.DfEUserId,
            ClosedVacanciesBlockedProviderAlertDismissedOn = source.ClosedVacanciesBlockedProviderAlertDismissedOn,
            TransferredVacanciesBlockedProviderAlertDismissedOn =
                source.TransferredVacanciesBlockedProviderAlertDismissedOn,
            ClosedVacanciesWithdrawnByQaAlertDismissedOn = source.ClosedVacanciesWithdrawnByQaAlertDismissedOn,
            TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn =
                source.TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn
        };
    }
}