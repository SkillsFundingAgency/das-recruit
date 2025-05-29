using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetCandidateDetailApiRequest : IGetApiRequest
{
    private readonly Guid _candidateId;

    public GetCandidateDetailApiRequest(Guid candidateId)
    {
        _candidateId = candidateId;
    }

    public string GetUrl => $"candidates/{_candidateId}";
}