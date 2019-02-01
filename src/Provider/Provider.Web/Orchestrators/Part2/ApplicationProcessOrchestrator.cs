﻿using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ApplicationProcess;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2
{
    public class ApplicationProcessOrchestrator : EntityValidatingOrchestrator<Vacancy, ApplicationProcessEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ApplicationMethod;
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly ExternalLinksConfiguration _externalLinks;
        private readonly IReviewSummaryService _reviewSummaryService;

        public ApplicationProcessOrchestrator(IProviderVacancyClient client,
            IRecruitVacancyClient vacancyClient,
            IOptions<ExternalLinksConfiguration> externalLinks, ILogger<ApplicationProcessOrchestrator> logger, 
            IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _externalLinks = externalLinks.Value;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<ApplicationProcessViewModel> GetApplicationProcessViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.ApplicationProcess_Get);

            var vm = new ApplicationProcessViewModel
            {
                Title = vacancy.Title,
                FindAnApprenticeshipUrl = _externalLinks.FindAnApprenticeshipUrl,
                ApplicationMethod = vacancy.ApplicationMethod,
                ApplicationInstructions = vacancy.ApplicationInstructions,
                ApplicationUrl = vacancy.ApplicationUrl
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                //vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                //    ReviewFieldMappingLookups.GetApplicationProcessFieldIndicators());
            }

            return vm;
        }

        public async Task<ApplicationProcessViewModel> GetApplicationProcessViewModelAsync(ApplicationProcessEditModel m)
        {
            var vm = await GetApplicationProcessViewModelAsync((VacancyRouteModel)m);

            vm.ApplicationMethod = m.ApplicationMethod;
            vm.ApplicationInstructions = m.ApplicationInstructions;
            vm.ApplicationUrl = m.ApplicationUrl;

            return vm;
        }

        public async Task<OrchestratorResponse> PostApplicationProcessEditModelAsync(ApplicationProcessEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.ApplicationProcess_Post);

            var hasSelectedApplyThroughFaa = m.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;

            vacancy.ApplicationMethod = m.ApplicationMethod;
            vacancy.ApplicationInstructions = hasSelectedApplyThroughFaa ? null : m.ApplicationInstructions;
            vacancy.ApplicationUrl = hasSelectedApplyThroughFaa ? null : m.ApplicationUrl;

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ApplicationProcessEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, ApplicationProcessEditModel>
            {
                { e => e.ApplicationMethod, vm => vm.ApplicationMethod },
                { e => e.ApplicationUrl, vm => vm.ApplicationUrl },
                { e => e.ApplicationInstructions, vm => vm.ApplicationInstructions }
            };

            return mappings;
        }
    }
}
