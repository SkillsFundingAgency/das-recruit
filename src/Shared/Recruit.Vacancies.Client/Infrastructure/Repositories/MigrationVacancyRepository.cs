using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;

// TODO: Delegates interface calls to the correct implementation(s) - this class should go once we have migrated vacancies over to SQL
public class MigrationVacancyRepository(
    ILogger<MigrationVacancyRepository> logger,
    [FromKeyedServices("mongo")] IVacancyRepository mongoRepository,
    [FromKeyedServices("sql")] IVacancyRepository sqlRepository): IVacancyRepository
{
    public async Task CreateAsync(Vacancy vacancy)
    {
        await mongoRepository.CreateAsync(vacancy);
        try
        {
            await sqlRepository.CreateAsync(vacancy);
        }
        catch (Exception ex)
        {
            // let's swallow exceptions until the creates are working smoothly
            logger.LogError(ex, "Error calling the migration SQL repository to CREATE a vacancy");
        }
    }

    public async Task UpdateAsync(Vacancy vacancy)
    {
        await mongoRepository.UpdateAsync(vacancy);
        try
        {
            await sqlRepository.UpdateAsync(vacancy);
        }
        catch (Exception ex)
        {
            // let's swallow exceptions until the updates are working smoothly
            logger.LogError(ex, "Error calling the migration SQL repository to UPDATE a vacancy");
        }
    }

    public Task<Vacancy> GetVacancyAsync(Guid id)
    {
        return mongoRepository.GetVacancyAsync(id);
    }

    public Task<Vacancy> GetVacancyAsync(long vacancyReference)
    {
        return mongoRepository.GetVacancyAsync(vacancyReference);
    }
}