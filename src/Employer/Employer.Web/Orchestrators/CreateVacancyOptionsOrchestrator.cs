using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Employer.Web.ViewModels.CreateVacancyOptions;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;
using System.Linq;
using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class CreateVacancyOptionsOrchestrator
    {
        private readonly IEmployerVacancyClient _vacancyClient;

        public CreateVacancyOptionsOrchestrator(IEmployerVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }

        public async Task<CreateVacancyOptionsViewModel> GetCreateOptionsViewModelAsync(string employerAccountId)
        {
            var dashboard = await _vacancyClient.GetDashboardAsync(employerAccountId);
            var vm = MapFromDashboard(dashboard);
            
            return vm;
        }

        public async Task<Guid> CloneVacancy(string employerAccountId, Guid vacancyId, VacancyUser user)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vacancyId);

            Utility.CheckAuthorisedAccess(vacancy, employerAccountId);

            var clonedVacancyId = await _vacancyClient.CloneVacancyAsync(vacancyId, user);

            return clonedVacancyId;
        }

        private static CreateVacancyOptionsViewModel MapFromDashboard(EmployerDashboard dashboard)
        {
            var summaries = dashboard.CloneableVacancies.Select(x => new ClonableVacancy 
                {
                    Id = x.Id,
                    VacancyReference = x.VacancyReference.Value,
                    Summary = BuildSummaryText(x)
                });

            return new CreateVacancyOptionsViewModel
            {
                Vacancies = summaries
            };
        }

        private static string BuildSummaryText(VacancySummary x)
        {
            return $"{x.Title}, {x.ClosingDate.Value.AsGdsDate()}, status: {x.Status.GetDisplayName()}, {x.TrainingTitle}, Level: {x.TrainingLevel.GetDisplayName()} ({x.TrainingType.GetDisplayName()})";
        }
    }
}
