using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IEmployerVacancyClient
    {
        Task<Guid> CreateVacancyAsync(SourceOrigin origin, string title, int numberOfPositions, string employerAccountId, VacancyUser user, UserType userType);
        Task GenerateDashboard(string employerAccountId);
        Task CloseVacancyAsync(Guid vacancyId, VacancyUser user);
        Task<Guid> CloneVacancyAsync(Guid vacancyId, VacancyUser user, SourceOrigin sourceOrigin);
        
        Task UpdatePublishedVacancyAsync(Vacancy vacancy, VacancyUser user);
        Task SubmitVacancyAsync(Guid vacancyId, string employerDescription, VacancyUser user);
        Task DeleteVacancyAsync(Guid vacancyId, VacancyUser user);
        Task<EmployerDashboard> GetDashboardAsync(string employerAccountId);
        Task<EditVacancyInfo> GetEditVacancyInfoAsync(string employerAccountId);
        
        Task<IEnumerable<IApprenticeshipProgramme>> GetActiveApprenticeshipProgrammesAsync();
        Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId);
        Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string employerAccountId);
        Task<IEnumerable<string>> GetEmployerIdentifiersAsync(string userId);
        Task SetupEmployerAsync(string employerAccountId);
        Task<List<string>> GetCandidateSkillsAsync();
        Task<IList<string>> GetCandidateQualificationsAsync();
        Task<EmployerProfile> GetEmployerProfileAsync(string employerAccountId, long legalEntityId);
        Task UpdateEmployerProfileAsync(EmployerProfile employerProfile, VacancyUser user);
        
        Task<VacancyApplications> GetVacancyApplicationsAsync(string vacancyReference);
        Task SetApplicationReviewSuccessful(Guid applicationReviewId, VacancyUser user);
        Task SetApplicationReviewUnsuccessful(Guid applicationReviewId, string candidateFeedback, VacancyUser user);
        Task<User> GetUsersDetailsAsync(string userId);
        Task SaveLevyDeclarationAsync(string userId, string employerAccountId);
    }
}