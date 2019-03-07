using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IRecruitVacancyClient
    {
        Task AssignVacancyNumber(Guid vacancyId);
        Task UserSignedInAsync(VacancyUser user, UserType userType);
        Task<Vacancy> GetVacancyAsync(Guid vacancyId);
        Task<VacancyReview> GetCurrentReferredVacancyReviewAsync(long vacancyReference);
        Task<List<string>> GetCandidateSkillsAsync();
        Task<IList<string>> GetCandidateQualificationsAsync();
        Task<ApplicationReview> GetApplicationReviewAsync(Guid applicationReviewId);
        EntityValidationResult Validate(Vacancy vacancy, VacancyRuleSet rules);
        Task UpdateDraftVacancyAsync(Vacancy vacancy, VacancyUser user);
        Task<IEnumerable<IApprenticeshipProgramme>> GetActiveApprenticeshipProgrammesAsync();
        Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId);
        Task<VacancyApplications> GetVacancyApplicationsAsync(string vacancyReference);
        Task UpdatePublishedVacancyAsync(Vacancy vacancy, VacancyUser user);
        Task<Guid> CloneVacancyAsync(Guid vacancyId, VacancyUser user, SourceOrigin sourceOrigin, DateTime startDate, DateTime closingDate);
    }
}