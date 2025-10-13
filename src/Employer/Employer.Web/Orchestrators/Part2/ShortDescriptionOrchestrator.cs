using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.ShortDescription;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class ShortDescriptionOrchestrator : VacancyValidatingOrchestrator<ShortDescriptionEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ShortDescription;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IUtility _utility;

        public ShortDescriptionOrchestrator(IRecruitVacancyClient vacancyClient, ILogger<ShortDescriptionOrchestrator> logger, IReviewSummaryService reviewSummaryService, IUtility utility) : base(logger)
        {
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _utility = utility;
        }

        public async Task<ShortDescriptionViewModel> GetShortDescriptionViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm);

            var vm = new ShortDescriptionViewModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vacancy.EmployerAccountId,
                ShortDescription = vacancy.ShortDescription,
                Title = vacancy.Title
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value, 
                    ReviewFieldMappingLookups.GetShortDescriptionReviewFieldIndicators());
            }
            
            vm.IsTaskListCompleted = _utility.IsTaskListCompleted(vacancy);

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
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m);

            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.ShortDescription,
                FieldIdResolver.ToFieldId(v => v.ShortDescription),
                vacancy,
                (v) =>
                {
                    return v.ShortDescription = m.ShortDescription;
                });

            return await ValidateAndExecute(
                vacancy, 
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ShortDescriptionEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, ShortDescriptionEditModel>
            {
                { e => e.ShortDescription, vm => vm.ShortDescription }
            };

            return mappings;
        }
    }
}
