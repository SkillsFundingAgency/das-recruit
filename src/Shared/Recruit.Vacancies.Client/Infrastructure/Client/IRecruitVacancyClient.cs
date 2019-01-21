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
    public interface IRecruitVacancyClient
    {
         Task UserSignedInAsync(VacancyUser user, UserType userType);
         Task<Vacancy> GetVacancyAsync(Guid vacancyId);
         Task<VacancyReview> GetCurrentReferredVacancyReviewAsync(long vacancyReference);

         Task<ApplicationReview> GetApplicationReviewAsync(Guid applicationReviewId);

         EntityValidationResult Validate(Vacancy vacancy, VacancyRuleSet rules);

         Task UpdateDraftVacancyAsync(Vacancy vacancy, VacancyUser user);
    }
}