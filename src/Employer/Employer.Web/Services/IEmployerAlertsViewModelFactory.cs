using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Alerts;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Services;

public interface IEmployerAlertsViewModelFactory
{
    Task<AlertsViewModel> Create(string employerAccountId, User user);
}