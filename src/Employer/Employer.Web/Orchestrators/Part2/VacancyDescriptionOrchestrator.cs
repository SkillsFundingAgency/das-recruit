using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class VacancyDescriptionOrchestrator : EntityValidatingOrchestrator<Vacancy, VacancyDescriptionEditModel>
    {
        private const VacancyRuleSet ValdationRules = VacancyRuleSet.Description | VacancyRuleSet.TrainingDescription | VacancyRuleSet.OutcomeDescription;
        private readonly IEmployerVacancyClient _client;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IHtmlSanitizerService _htmlSanitizerService;

        public VacancyDescriptionOrchestrator(
            IEmployerVacancyClient client, 
            ILogger<VacancyDescriptionOrchestrator> logger, 
            IReviewSummaryService reviewSummaryService,
            IHtmlSanitizerService htmlSanitizerService) : base(logger)
        {
            _client = client;
            _reviewSummaryService = reviewSummaryService;
            _htmlSanitizerService = htmlSanitizerService;
        }

        public async Task<VacancyDescriptionViewModel> GetVacancyDescriptionViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, vrm, RouteNames.VacancyDescription_Index_Get);

            var vm = new VacancyDescriptionViewModel
            {
                Title = vacancy.Title,
                VacancyDescription = vacancy.Description,
                TrainingDescription = vacancy.TrainingDescription,
                OutcomeDescription = vacancy.OutcomeDescription
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModel(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetVacancyDescriptionFieldIndicators());
            }

            return vm;
        }

        public async Task<VacancyDescriptionViewModel> GetVacancyDescriptionViewModelAsync(VacancyDescriptionEditModel m)
        {
            var vm = await GetVacancyDescriptionViewModelAsync((VacancyRouteModel)m);

            vm.VacancyDescription = m.VacancyDescription;
            vm.TrainingDescription = m.TrainingDescription;
            vm.OutcomeDescription = m.OutcomeDescription;

            return vm;
        }

        public async Task<OrchestratorResponse> PostVacancyDescriptionEditModelAsync(VacancyDescriptionEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, m, RouteNames.VacancyDescription_Index_Post);
            
            vacancy.Description = _htmlSanitizerService.Sanitize(m.VacancyDescription);
            vacancy.TrainingDescription = _htmlSanitizerService.Sanitize(m.TrainingDescription);
            vacancy.OutcomeDescription = _htmlSanitizerService.Sanitize(m.OutcomeDescription);
            
            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValdationRules),
                v => _client.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, VacancyDescriptionEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, VacancyDescriptionEditModel>();

            mappings.Add(e => e.Description, vm => vm.VacancyDescription);
            mappings.Add(e => e.TrainingDescription, vm => vm.TrainingDescription);
            mappings.Add(e => e.OutcomeDescription, vm => vm.OutcomeDescription);

            return mappings;
        }
    }
}
