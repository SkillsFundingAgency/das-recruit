﻿using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Employer.Web.ViewModels.CreateVacancy;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;
using System.Linq;

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
            var summaries = dashboard.CloneableVacancies.Select(x => new ClonableVacancy 
                {
                    Id = x.Id,
                    VacancyReference = x.VacancyReference.Value,
                    Summary = BuildSummary(x)
                });

            return new CreateOptionsViewModel
            {
                Vacancies = summaries
            };
        }

        private static string BuildSummary(VacancySummary x)
        {
            return $"{x.Title}, {x.ClosingDate.Value.AsGdsDate()}, status: {x.Status.GetDisplayName()}, {x.TrainingTitle}, Level: {x.TrainingLevel.GetDisplayName()} ({x.TrainingType.GetDisplayName()})";
        }
    }
}
