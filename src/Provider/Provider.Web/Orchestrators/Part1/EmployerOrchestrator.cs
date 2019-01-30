using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class EmployerOrchestrator 
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerAccountId;
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;

        public EmployerOrchestrator(ILogger<EmployerOrchestrator> logger, 
            IProviderVacancyClient providerVacancyClient, IRecruitVacancyClient recruitVacancyClient)
            
        {
            _providerVacancyClient = providerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
        }

        public async Task<EmployersViewModel> GetEmployersViewModelAsync(VacancyRouteModel vacancyRouteModel)
        {
            var editVacancyInfo = await _providerVacancyClient.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn);

            var vm = new EmployersViewModel
            {
                PageInfo = new PartOnePageInfoViewModel(),
                Employers = editVacancyInfo.Employers.Select(e => new EmployerViewModel {Id = e.Id, Name = e.Name})
            };

            return vm;
        }
    }
}