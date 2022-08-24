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
        Task GenerateDashboard(string employerAccountId);
        Task DeleteVacancyAsync(Guid vacancyId, VacancyUser user);
        Task<EmployerDashboard> GetDashboardAsync(string employerAccountId, bool createIfNonExistent = false);
        Task<EmployerEditVacancyInfo> GetEditVacancyInfoAsync(string employerAccountId);
        Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string employerAccountId);
        Task SetupEmployerAsync(string employerAccountId);
        Task SetApplicationReviewSuccessful(Guid applicationReviewId, VacancyUser user);
        Task SetApplicationReviewUnsuccessful(Guid applicationReviewId, string candidateFeedback, VacancyUser user);        
        Task<int> GetVacancyCountForUserAsync(string userId);
        EntityValidationResult ValidateQualification(Qualification qualification);

        Task CreateEmployerApiVacancy(Guid id, string title, string employerAccountId, VacancyUser user, TrainingProvider provider, string programmeId);
    }
}