﻿using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Services;
using Microsoft.Extensions.Options;
using Esfa.Recruit.Shared.Web.FeatureToggle;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class ApplicationProcessOrchestrator : EntityValidatingOrchestrator<Vacancy, ApplicationProcessEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ApplicationMethod;
        private readonly IEmployerVacancyClient _client;
        private readonly ExternalLinksConfiguration _externalLinks;
        private readonly IFeature _featureToggler;
        private readonly IReviewSummaryService _reviewSummaryService;

        public ApplicationProcessOrchestrator(IEmployerVacancyClient client, IOptions<ExternalLinksConfiguration> externalLinks, ILogger<ApplicationProcessOrchestrator> logger, IFeature featureToggler, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _externalLinks = externalLinks.Value;
            _featureToggler = featureToggler;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<ApplicationProcessViewModel> GetApplicationProcessViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, vrm, RouteNames.ApplicationProcess_Get);

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
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModel(vacancy.VacancyReference.Value,
                    ReviewFieldIndicatorMapper.ApplicationProcessFieldIndicators);
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
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, m, RouteNames.ApplicationProcess_Post);

            if (_featureToggler.IsFeatureEnabled(FeatureNames.AllowThroughFaaApplicationMethod))
            {
                var hasSelectedApplyThroughFaa = m.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;

                vacancy.ApplicationMethod = m.ApplicationMethod;
                vacancy.ApplicationInstructions = hasSelectedApplyThroughFaa ? null : m.ApplicationInstructions;
                vacancy.ApplicationUrl = hasSelectedApplyThroughFaa ? null : m.ApplicationUrl;
            }
            else
            {
                vacancy.ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite;
                vacancy.ApplicationInstructions = m.ApplicationInstructions;
                vacancy.ApplicationUrl = m.ApplicationUrl;
            }

            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateDraftVacancyAsync(vacancy, user)
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
