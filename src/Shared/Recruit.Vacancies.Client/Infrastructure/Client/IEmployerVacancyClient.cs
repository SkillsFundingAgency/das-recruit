using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IEmployerVacancyClient
    {
        Task<Guid> CreateVacancyAsync(string title, string employerAccountId, VacancyUser user, TrainingProvider provider = null, string programmeId = null);
        Task DeleteVacancyAsync(Guid vacancyId, VacancyUser user);
        Task<Vacancy> GetVacancyAsync(Guid vacancyId);
        Task<EmployerDashboard> GetDashboardAsync(string employerAccountId,int page, FilteringOptions? status = null, string searchTerm = null);
        Task<EmployerEditVacancyInfo> GetEditVacancyInfoAsync(string employerAccountId);
        Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string employerAccountId);
        Task SetupEmployerAsync(string employerAccountId);
        Task SetApplicationReviewStatus(Guid applicationReviewId, ApplicationReviewStatus? outcome, string candidateFeedback, VacancyUser user);
        Task<int> GetVacancyCountForUserAsync(string userId);
        EntityValidationResult ValidateQualification(Qualification qualification);

        Task CreateEmployerApiVacancy(Guid id, string title, string employerAccountId, VacancyUser user, TrainingProvider provider, string programmeId);
        Task<long> GetVacancyCount(string employerAccountId, VacancyType vacancyType, FilteringOptions? filteringOptions, string searchTerm);
        Task<EmployerDashboardSummary> GetDashboardSummary(string employerAccountId);
    }
}