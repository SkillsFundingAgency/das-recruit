using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ShortDescription;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class ShortDescriptionOrchestrator : VacancyValidatingOrchestrator<ShortDescriptionEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ShortDescription;
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public ShortDescriptionOrchestrator(IProviderVacancyClient providerVacnacyClient, 
            IRecruitVacancyClient recruitVacancyClient, ILogger<ShortDescriptionOrchestrator> logger, 
            IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _providerVacancyClient = providerVacnacyClient;
            _recruitVacancyClient = recruitVacancyClient;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<ShortDescriptionViewModel> GetShortDescriptionViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_providerVacancyClient, 
                _recruitVacancyClient, vrm, RouteNames.ShortDescription_Get);

            var vm = new ShortDescriptionViewModel
            {
                VacancyId = vacancy.Id,
                ShortDescription = vacancy.ShortDescription,
                Title = vacancy.Title
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetShortDescriptionReviewFieldIndicators());
            }

            return vm;
        }

        public async Task<ShortDescriptionViewModel> GetShortDescriptionViewModelAsync(ShortDescriptionEditModel m)
        {
            var vm = await GetShortDescriptionViewModelAsync((VacancyRouteModel)m);
            
            vm.ShortDescription = m.ShortDescription;

            return vm;
        }

        public async Task<OrchestratorResponse> PostShortDescriptionEditModelAsync(ShortDescriptionEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_providerVacancyClient, 
                _recruitVacancyClient, m, RouteNames.ShortDescription_Post);

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.ShortDescription,
                FieldIdResolver.ToFieldId(v => v.ShortDescription),
                vacancy,
                (v) =>
                {
                    return v.ShortDescription = m.ShortDescription;
                });

            return await ValidateAndExecute(
                vacancy, 
                v => _recruitVacancyClient.Validate(v, ValidationRules),
                v => _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ShortDescriptionEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, ShortDescriptionEditModel>();

            mappings.Add(e => e.ShortDescription, vm => vm.ShortDescription);

            return mappings;
        }
    }

}