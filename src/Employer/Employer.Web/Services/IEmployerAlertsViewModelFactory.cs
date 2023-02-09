using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Alerts;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface IEmployerAlertsViewModelFactory
    {
        Task<AlertsViewModel> Create(string employerAccountId, User user);
    }
}