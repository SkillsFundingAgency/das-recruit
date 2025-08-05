namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public record GetCourseProvidersRequest(int LarsCode) : IGetApiRequest
{
    public string GetUrl => $"courses/{LarsCode}/providers";
}