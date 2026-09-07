using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Providers;

public record GetEmployerPermissionsByAccountHashedIdApiRequest(string AccountHashedId, List<OperationType> Operations) : IGetApiRequest
{
    public string GetUrl => $"accountLegalEntities/employerAccount/{AccountHashedId}?operations={string.Join("&operations=", Operations)}";
}