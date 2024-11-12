using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.VacancyDescription;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2
{
    public class VacancyDescriptionOrchestrator : VacancyValidatingOrchestrator<VacancyDescriptionEditModel>
    {
        private readonly VacancyRuleSet ValidationRules;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IUtility _utility;
        private readonly IFeature _feature;
        private readonly ServiceParameters _serviceParameters;

        public VacancyDescriptionOrchestrator(IRecruitVacancyClient vacancyClient,
            ILogger<VacancyDescriptionOrchestrator> logger, 
            IReviewSummaryService reviewSummaryService,
            IUtility utility, 
            IFeature feature,
            ServiceParameters serviceParameters) : base(logger)
        {
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _utility = utility;
            _feature = feature;
            _serviceParameters = serviceParameters;
            ValidationRules = _serviceParameters.VacancyType == VacancyType.Apprenticeship 
                ? VacancyRuleSet.Description | VacancyRuleSet.TrainingDescription
                : VacancyRuleSet.TrainingDescription;
        }

        public async Task<VacancyDescriptionViewModel> GetVacancyDescriptionViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.VacancyDescription_Index_Get);

            var vm = new VacancyDescriptionViewModel
            {
                Title = vacancy.Title,
                TrainingDescription = vacancy.TrainingDescription,
                Ukprn = vrm.Ukprn,
                VacancyId = vrm.VacancyId
            };

            if (_serviceParameters.VacancyType == VacancyType.Apprenticeship)
            {
                vm.VacancyDescription = vacancy.Description;
            }

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                   ReviewFieldMappingLookups.GetVacancyDescriptionFieldIndicators());
            }

            vm.IsTaskListCompleted = _utility.IsTaskListCompleted(vacancy);

            return vm;
        }

        public async Task<VacancyDescriptionViewModel> GetVacancyDescriptionViewModelAsync(VacancyDescriptionEditModel m)
        {
            var vm = await GetVacancyDescriptionViewModelAsync((VacancyRouteModel)m);

            if (_serviceParameters.VacancyType == VacancyType.Apprenticeship)
            {
                vm.VacancyDescription = m.VacancyDescription;
            }
            vm.TrainingDescription = m.TrainingDescription;
            
            return vm;
        }

        public async Task<OrchestratorResponse> PostVacancyDescriptionEditModelAsync(VacancyDescriptionEditModel m, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.VacancyDescription_Index_Post);
            
            if (_serviceParameters.VacancyType is VacancyType.Apprenticeship)
            {
                SetVacancyWithProviderReviewFieldIndicators(
                    vacancy.Description,
                    FieldIdResolver.ToFieldId(v => v.Description),
                    vacancy,
                    (v) => { return v.Description = m.VacancyDescription; });
            }

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.TrainingDescription,
                FieldIdResolver.ToFieldId(v => v.TrainingDescription),
                vacancy,
                (v) => { return v.TrainingDescription = m.TrainingDescription; });

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, VacancyDescriptionEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, VacancyDescriptionEditModel>();

            if (_serviceParameters.VacancyType == VacancyType.Apprenticeship)
            {
                mappings.Add(e => e.Description, vm => vm.VacancyDescription);   
            }
            mappings.Add(e => e.TrainingDescription, vm => vm.TrainingDescription);

            return mappings;
        }
    }
}
