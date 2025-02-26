using System;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public abstract class VacancyValidatingOrchestrator<TEditModel>(ILogger logger) : EntityValidatingOrchestrator<Vacancy, TEditModel>(logger)
    {
        private readonly ReviewFieldIndicatorService _reviewFieldIndicatorService = new ();

        protected void SetVacancyWithEmployerReviewFieldIndicators<T>(T currentValue, string fieldId, Vacancy vacancy, Func<Vacancy, T> setFunc)
        {
            var newValue = setFunc(vacancy);
            _reviewFieldIndicatorService.SetVacancyWithEmployerReviewFieldIndicators(currentValue, fieldId, vacancy, newValue);
        }
    }
}
