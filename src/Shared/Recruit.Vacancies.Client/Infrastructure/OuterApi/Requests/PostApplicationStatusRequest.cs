using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class PostApplicationStatusRequest(Guid candidateId, Guid applicationId, PostApplicationStatus postApplicationStatus) : IPostApiRequest
{
    public string PostUrl => $"candidates/{candidateId}applications/{applicationId}/status";
    public object Data { get; set; }= postApplicationStatus;
}

public class PostApplicationStatus
{
    public string Status { get; set; }
    public string CandidateFeedback { get; set; }
}