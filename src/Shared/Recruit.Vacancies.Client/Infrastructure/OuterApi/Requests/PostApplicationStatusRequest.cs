using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class PostApplicationStatusRequest(Guid candidateId, Guid applicationId, PostApplicationStatus postApplicationStatus) : IPostApiRequest
{
    public string PostUrl => $"candidates/{candidateId}/applications/{applicationId}";
    public object Data { get; set; } = postApplicationStatus;
}

public class PostApplicationStatus
{
    public string Status { get; set; }
    public string CandidateFeedback { get; set; }
    public long VacancyReference { get; set; }
    public string VacancyTitle { get; set; }
    public string VacancyEmployerName { get; set; }
    public string VacancyLocation { get; set; }
}