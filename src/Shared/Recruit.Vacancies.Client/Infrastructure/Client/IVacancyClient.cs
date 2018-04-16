using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IVacancyClient
    {
        Task<Vacancy> GetVacancyAsync(Guid id);
        Task<Guid> CreateVacancyAsync(string title, string employerAccountId, string user);
        Task UpdateVacancyAsync(Vacancy vacancy);
        Task<bool> SubmitVacancyAsync(Guid id);
        Task<bool> DeleteVacancyAsync(Guid id);
        Task<Dashboard> GetDashboardAsync(string employerAccountId);
        Task RecordEmployerAccountSignInAsync(string employerAccountId);
        Task UpdateApprenticeshipProgrammesAsync(IEnumerable<ApprenticeshipProgramme> programmes);
        Task<ApprenticeshipProgrammes> GetApprenticeshipProgrammesAsync();
        Task<EditVacancyInfo> GetEditVacancyInfo(string employerAccountId);
        EntityValidationResult Validate(Vacancy vacancy, VacancyRuleSet rules);
        Task AssignVacancyNumber(Guid id);
    }
}