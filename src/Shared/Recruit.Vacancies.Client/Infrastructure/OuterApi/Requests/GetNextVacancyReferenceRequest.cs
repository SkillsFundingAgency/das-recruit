namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetNextVacancyReferenceRequest: IGetApiRequest
{
    public string GetUrl => "vacancies/vacancyreference";
}