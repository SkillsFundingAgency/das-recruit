using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Skills;
using Quals = Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Qualifications;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IEmployerVacancyClient
    {
        Task<Vacancy> GetVacancyAsync(Guid vacancyId);
        Task<Guid> CreateVacancyAsync(SourceOrigin origin, string title, int numberOfPositions, string employerAccountId, VacancyUser user);
        Task UpdateDraftVacancyAsync(Vacancy vacancy, VacancyUser user);
        Task UpdatePublishedVacancyAsync(Vacancy vacancy, VacancyUser user);
        Task SubmitVacancyAsync(Guid vacancyId, VacancyUser user);
        Task DeleteVacancyAsync(Guid vacancyId, VacancyUser user);
        Task<EmployerDashboard> GetDashboardAsync(string employerAccountId);
        Task UserSignedInAsync(VacancyUser user);
        Task<EditVacancyInfo> GetEditVacancyInfo(string employerAccountId);
        EntityValidationResult Validate(Vacancy vacancy, VacancyRuleSet rules);
        Task<IEnumerable<IApprenticeshipProgramme>> GetActiveApprenticeshipProgrammesAsync();
        Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId);
        Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string employerAccountId);
        Task<IEnumerable<string>> GetEmployerIdentifiersAsync(string userId);
        Task SetupEmployerAsync(string employerAccountId);
        Task<List<string>> GetCandidateSkillsAsync();
        Task<Quals.Qualifications> GetCandidateQualificationsAsync();
        Task<ApplicationReview> GetApplicationReviewAsync(Guid applicationReviewId);
        Task<VacancyApplications> GetVacancyApplicationsAsync(string vacancyReference);
        Task SetApplicationReviewSuccessful(Guid applicationReviewId, VacancyUser user);
        Task SetApplicationReviewUnsuccessful(Guid applicationReviewId, string candidateFeedback, VacancyUser user);
        Task<VacancyReview> GetVacancyReviewAsync(long vacancyReference);
    }
}