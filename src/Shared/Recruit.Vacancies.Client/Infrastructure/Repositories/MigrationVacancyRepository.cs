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
        await sqlRepository.CreateAsync(vacancy);
        try
        {
            await mongoRepository.CreateAsync(vacancy);
        }
        catch (Exception ex)
        {
            // swallow errors
            logger.LogError(ex, "Error calling the migration Mongo repository to CREATE a vacancy");
        }
    }

    public async Task UpdateAsync(Vacancy vacancy)
    {
        await sqlRepository.UpdateAsync(vacancy);
        try
        {
            await mongoRepository.UpdateAsync(vacancy);
        }
        catch (Exception ex)
        {
            // swallow errors
            logger.LogError(ex, "Error calling the migration Mongo repository to UPDATE a vacancy");
        }
    }

    public async Task<Vacancy> GetVacancyAsync(Guid id)
    {
        return await sqlRepository.GetVacancyAsync(id);
    }

    public async Task<Vacancy> GetVacancyAsync(long vacancyReference)
    {
        return await sqlRepository.GetVacancyAsync(vacancyReference);
    }
}