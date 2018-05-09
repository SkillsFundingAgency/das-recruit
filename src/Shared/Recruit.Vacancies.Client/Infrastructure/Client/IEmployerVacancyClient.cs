using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.Models;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Dashboard;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IEmployerVacancyClient
    {
        Task<Vacancy> GetVacancyAsync(Guid vacancyId);
        Task<Guid> CreateVacancyAsync(SourceOrigin origin, string title, string employerAccountId, VacancyUser user);
        Task UpdateVacancyAsync(Vacancy vacancy, VacancyUser user);
        Task SubmitVacancyAsync(Guid vacancyId, VacancyUser user);
        Task DeleteVacancyAsync(Guid vacancyId, VacancyUser user);
        Task<Dashboard> GetDashboardAsync(string employerAccountId);
        Task RecordEmployerAccountSignInAsync(string employerAccountId);
        Task<EditVacancyInfo> GetEditVacancyInfo(string employerAccountId);
        EntityValidationResult Validate(Vacancy vacancy, VacancyRuleSet rules);
        Task<IEnumerable<IApprenticeshipProgramme>> GetActiveApprenticeshipProgrammesAsync();
        Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId);
        Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string employerAccountId);
        Task<IEnumerable<string>> GetEmployerIdentifiersAsync(string userId);
    }
}