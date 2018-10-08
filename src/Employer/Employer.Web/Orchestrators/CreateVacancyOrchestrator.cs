using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Employer.Web.ViewModels.CreateVacancy;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;
using System.Linq;
using System;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class CreateVacancyOrchestrator
    {
        private readonly IEmployerVacancyClient _vacancyClient;

        public CreateVacancyOrchestrator(IEmployerVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }

        public async Task<CreateOptionsViewModel> GetCreateOptionsViewModelAsync(string employerAccountId)
        {
            var dashboard = await _vacancyClient.GetDashboardAsync(employerAccountId);
            var vm = MapFromDashboard(dashboard);
            
            return vm;
        }

        private static CreateOptionsViewModel MapFromDashboard(EmployerDashboard dashboard)
        {
            return new CreateOptionsViewModel
            {
                Vacancies = dashboard.CloneableVacancies.Select(x => new ClonableVacancy 
                {
                    Id = x.Id,
                    VacancyReference = x.VacancyReference.Value,
                    Summary = BuildSummary(x)
                })
            };
        }

        private static string BuildSummary(VacancySummary x)
        {
            return $"{x.Title}, {x.ClosingDate.Value.AsGdsDate()}, status: {x.Status.GetDisplayName()},";
        }
    }
}
