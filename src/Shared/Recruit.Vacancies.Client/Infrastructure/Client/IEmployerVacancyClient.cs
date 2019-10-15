using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IEmployerVacancyClient
    {
        Task<Guid> CreateVacancyAsync(string title, string employerAccountId, VacancyUser user);
        Task GenerateDashboard(string employerAccountId);
        Task SubmitVacancyAsync(Guid vacancyId, string employerDescription, VacancyUser user);
        Task DeleteVacancyAsync(Guid vacancyId, VacancyUser user);
        Task<EmployerDashboard> GetDashboardAsync(string employerAccountId, bool createIfNonExistent = false);
        Task<EditVacancyInfo> GetEditVacancyInfoAsync(string employerAccountId);
        Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string employerAccountId);
        Task SetupEmployerAsync(string employerAccountId);
        Task SetApplicationReviewSuccessful(Guid applicationReviewId, VacancyUser user);
        Task SetApplicationReviewUnsuccessful(Guid applicationReviewId, string candidateFeedback, VacancyUser user);        
        Task SaveLevyDeclarationAsync(string userId, string employerAccountId);
        Task<TrainingProvider> GetTrainingProviderAsync(long ukprn);
        Task<int> GetVacancyCountForUserAsync(string userId);
        EntityValidationResult ValidateQualification(Qualification qualification);
    }
}