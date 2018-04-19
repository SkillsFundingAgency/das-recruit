using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public abstract class VacancyValidatingOrchestrator<TEditModel> : EntityValidatingOrchestrator<Vacancy, TEditModel>
    {
        public VacancyValidatingOrchestrator(ILogger logger) : base(logger)
        {
        }

        public void CheckAuthorisedAccess(Vacancy vacancy, string employerAccountId)
        {
            if (!vacancy.EmployerAccountId.Equals(employerAccountId, StringComparison.OrdinalIgnoreCase))
                throw new AuthorisationException(string.Format(ExceptionMessages.VacancyUnauthorisedAccess, employerAccountId, vacancy.EmployerAccountId, vacancy.Title, vacancy.Id));
        }
    }
}
