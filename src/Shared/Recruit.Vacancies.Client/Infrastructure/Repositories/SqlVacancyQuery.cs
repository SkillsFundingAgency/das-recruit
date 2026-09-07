using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;

public class SqlVacancyQuery(IOuterApiVacancyClient outerApiVacancyClient): IVacancyQuery
{
    public async Task<IEnumerable<VacancyIdentifier>> GetVacanciesToCloseAsync(DateTime pointInTime)
    {
        return await outerApiVacancyClient.GetVacanciesToCloseAsync(pointInTime);
    }

    public async Task<int> GetVacancyCountForUserAsync(string userId)
    {
        return await outerApiVacancyClient.GetVacancyCountForUserAsync(userId);
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

    public async Task<IList<Vacancy>> FindClosedVacancies(IList<long> vacancyReferences)
    {
        return await outerApiVacancyClient.FindClosedVacanciesAsync(vacancyReferences);
    }

    public async Task<IEnumerable<ProviderVacancySummary>> GetVacanciesAssociatedToProvider(long ukprn)
    {
        return await outerApiVacancyClient.GetVacanciesAssociatedToProviderAsync(ukprn);
    }

    public async Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesForLegalEntityAsync(long ukprn, string accountLegalEntityPublicHashedId)
    {
        return await outerApiVacancyClient.GetProviderOwnedVacanciesForLegalEntityAsync(ukprn, accountLegalEntityPublicHashedId);
    }

    public async Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesInReviewAsync(long ukprn, string accountLegalEntityPublicHashedId)
    {
        return await outerApiVacancyClient.GetProviderOwnedVacanciesInReviewAsync(ukprn, accountLegalEntityPublicHashedId);
    }

    public async Task<IEnumerable<Vacancy>> GetProviderOwnedVacanciesForEmployerWithoutAccountLegalEntityPublicHashedIdAsync(long ukprn, string employerAccountId)
    {
        return await outerApiVacancyClient.GetProviderOwnedVacanciesForEmployerWithoutAccountLegalEntityPublicHashedIdAsync(ukprn, employerAccountId);
    }

    public async Task<IEnumerable<T>> GetDraftVacanciesCreatedBeforeAsync<T>(DateTime staleDate)
    {
        return await outerApiVacancyClient.GetDraftVacanciesCreatedBeforeAsync<T>(staleDate);
    }

    public async Task<IEnumerable<T>> GetReferredVacanciesSubmittedBeforeAsync<T>(DateTime staleDate)
    {
        return await outerApiVacancyClient.GetReferredVacanciesSubmittedBeforeAsync<T>(staleDate);
    }
}