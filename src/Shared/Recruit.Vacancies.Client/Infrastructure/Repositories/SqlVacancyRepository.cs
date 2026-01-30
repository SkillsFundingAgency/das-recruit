using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;

// TODO: Proxies calls to the new outer api endpoints - this class should go once we have migrated vacancies over to SQL
public class SqlVacancyRepository(IOuterApiVacancyClient outerApiVacancyClient): IVacancyRepository
{
    public async Task CreateAsync(Vacancy vacancy)
    {
        await outerApiVacancyClient.CreateAsync(vacancy);
    }

    public async Task UpdateAsync(Vacancy vacancy)
    {
        await outerApiVacancyClient.UpdateAsync(vacancy);
    }

    public Task<int> GetVacancyCountForUserAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task<Vacancy> GetVacancyAsync(Guid id)
    {
        var vacancy = await outerApiVacancyClient.GetVacancyAsync(id);
        return vacancy ?? throw new VacancyNotFoundException(string.Format(ExceptionMessages.VacancyWithIdNotFound, id));
    }

    public async Task<Vacancy> GetVacancyAsync(long vacancyReference)
    {
        var vacancy = await outerApiVacancyClient.GetVacancyAsync(vacancyReference);
        return vacancy ?? throw new VacancyNotFoundException(string.Format(ExceptionMessages.VacancyWithReferenceNotFound, vacancyReference));
    }
}