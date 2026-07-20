using System.Threading.Tasks;
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
    public class ShortDescriptionOrchestrator(
        IRecruitVacancyClient vacancyClient,
        ILogger<ShortDescriptionOrchestrator> logger,
        IReviewSummaryService reviewSummaryService,
        IUtility utility)
        : VacancyValidatingOrchestrator<ShortDescriptionEditModel>(logger)
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ShortDescription;

        public async Task<ShortDescriptionViewModel> GetShortDescriptionViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm);

            var vm = new ShortDescriptionViewModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vacancy.EmployerAccountId,
                ShortDescription = vacancy.ShortDescription,
                Title = vacancy.Title
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.GetValueOrDefault(), 
                    ReviewFieldMappingLookups.GetShortDescriptionReviewFieldIndicators());
            }
            
            vm.IsTaskListCompleted = utility.IsTaskListCompleted(vacancy);

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
            var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m);

            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.ShortDescription,
                FieldIdResolver.ToFieldId(v => v.ShortDescription),
                vacancy,
                v => v.ShortDescription = m.ShortDescription);

            return await ValidateAndExecute(
                vacancy, 
                v => vacancyClient.Validate(v, ValidationRules),
                v => vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ShortDescriptionEditModel> DefineMappings() =>
            new()
            {
                { e => e.ShortDescription, vm => vm.ShortDescription }
            };
    }
}