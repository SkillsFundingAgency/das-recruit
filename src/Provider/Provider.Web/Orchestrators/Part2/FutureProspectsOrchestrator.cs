using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.Considerations;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.FutureProspects;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2
{
    public class FutureProspectsOrchestrator : VacancyValidatingOrchestrator<FutureProspectsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.OutcomeDescription;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IUtility _utility;

        public FutureProspectsOrchestrator(IRecruitVacancyClient vacancyClient,
            ILogger<FutureProspectsOrchestrator> logger,
            IReviewSummaryService reviewSummaryService,
            IUtility utility) : base(logger)
        {
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _utility = utility;
        }

        public async Task<FutureProspectsViewModel> GetFutureProspectsViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy =
                await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.FutureProspects_Get);

            var vm = new FutureProspectsViewModel
            {
                Title = vacancy.Title,
                FutureProspects = vacancy.OutcomeDescription,
                Ukprn = vrm.Ukprn,
                VacancyId = vrm.VacancyId
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetFutureProspectsFieldIndicators());
            }

            vm.IsTaskListCompleted = _utility.TaskListCompleted(vacancy);

            return vm;
        }

        public async Task<FutureProspectsViewModel> GetFutureProspectsViewModelAsync(FutureProspectsEditModel m)
        {
            var vm = await GetFutureProspectsViewModelAsync((VacancyRouteModel)m);

            vm.FutureProspects = m.FutureProspects;

            return vm;
        }

        public async Task<OrchestratorResponse> PostFutureProspectsEditModelAsync(FutureProspectsEditModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.FutureProspects_Post);

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.OutcomeDescription,
                FieldIdResolver.ToFieldId(v => v.OutcomeDescription),
                vacancy,
                (v) => { return v.OutcomeDescription = m.FutureProspects; });

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, FutureProspectsEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, FutureProspectsEditModel>();

            mappings.Add(e => e.OutcomeDescription, vm => vm.FutureProspects);

            return mappings;
        }
    }
}
