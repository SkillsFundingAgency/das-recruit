namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetVacancyPreviewApiRequest(int standardId) : IGetApiRequest
{
    public string GetUrl => $"vacancypreview?standardId={standardId}";
}