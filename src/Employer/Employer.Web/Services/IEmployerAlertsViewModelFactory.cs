using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface IEmployerAlertsViewModelFactory
    {
        AlertsViewModel Create(IEnumerable<VacancySummary> vacancies, User user);
        Task<AlertsViewModel> Create(string employerAccountId, User user);
    }
}