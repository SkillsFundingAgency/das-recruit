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
        Task<Vacancy> GetVacancyAsync(Guid id);
        Task<Guid> CreateVacancyAsync(string title, string employerAccountId, string user);
        Task UpdateVacancyAsync(Vacancy vacancy);
        Task<bool> SubmitVacancyAsync(Guid id, string user, string userEmail);
        Task<bool> DeleteVacancyAsync(Guid id);
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