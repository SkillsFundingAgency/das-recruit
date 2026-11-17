using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetCandidateDetailApiRequest(Guid candidateId) : IGetApiRequest
{
    public string GetUrl => $"candidates/{candidateId}";
}