using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

public interface IOuterApiVacancyClient
{
    Task CreateAsync(Vacancy vacancy);
    Task UpdateAsync(Vacancy vacancy);
    Task<long> GetNextVacancyIdAsync();
}

public class OuterApiVacancyClient(
    IEncodingService encodingService,
    IOuterApiClient apimRecruitClient): IOuterApiVacancyClient
{
    public async Task CreateAsync(Vacancy vacancy)
    {
        // TODO: we'll want the returned data here at some point
        await apimRecruitClient.Post(new PostVacancyRequest(vacancy.Id, VacancyDto.From(vacancy, encodingService)));
    }

    public async Task UpdateAsync(Vacancy vacancy)
    {
        // TODO: we'll want the returned data here at some point
        await apimRecruitClient.Post(new PostVacancyRequest(vacancy.Id, VacancyDto.From(vacancy, encodingService)));
    }

    public async Task<long> GetNextVacancyIdAsync()
    {
        var response = await apimRecruitClient.Get<GetNextVacancyReferenceResponse>(new GetNextVacancyReferenceRequest());
        return response.NextVacancyReference;
    }
}