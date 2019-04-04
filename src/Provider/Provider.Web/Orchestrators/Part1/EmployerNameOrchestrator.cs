using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.Model;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class EmployerNameOrchestrator : EntityValidatingOrchestrator<Vacancy, EmployerNameEditModel>
    {
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IEmployerVacancyClient _employerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public EmployerNameOrchestrator(
            IProviderVacancyClient providerVacancyClient, 
            IEmployerVacancyClient employerVacancyClient,
            IRecruitVacancyClient recruitVacancyClient, 
            ILogger<EmployerNameOrchestrator> logger, 
            IReviewSummaryService reviewSummaryService)
            : base(logger) 
        {
            _providerVacancyClient = providerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _employerVacancyClient = employerVacancyClient;
        }

        public async Task<EmployerNameViewModel> GetEmployerNameViewModelAsync(
            VacancyRouteModel vrm, VacancyEmployerInfoModel employerInfoModel, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.EmployerName_Get);

            var legalEntityId = employerInfoModel.LegalEntityId.GetValueOrDefault();
                
            var getVacancyEditInfoTask = _providerVacancyClient.GetProviderEditVacancyInfoAsync(vrm.Ukprn);
            var getEmployerProfileTask = _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, legalEntityId);
            await Task.WhenAll(getVacancyEditInfoTask, getEmployerProfileTask);
            var employerInfo = getVacancyEditInfoTask.Result.Employers.Single(e => e.EmployerAccountId == vacancy.EmployerAccountId);
            var employerProfile = getEmployerProfileTask.Result;

            var legalEntity = employerInfo.LegalEntities.Single(l => l.LegalEntityId == legalEntityId);

            var vm = new EmployerNameViewModel 
            {
                HasOnlyOneOrganisation = employerInfo.LegalEntities.Count() == 1,
                LegalEntityName = legalEntity.Name,
                ExistingTradingName = employerProfile?.TradingName,
                PageInfo = Utility.GetPartOnePageInfo(vacancy),
                SelectedEmployerNameOption = employerInfoModel.EmployerNameOption,
                NewTradingName = employerInfoModel.NewTradingName
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value, 
                    ReviewFieldMappingLookups.GetEmployerNameReviewFieldIndicators());
            }

            return vm;
        }

        public async Task<OrchestratorResponse> PostEmployerNameEditModelAsync(
            EmployerNameEditModel model, VacancyEmployerInfoModel employerInfoModel, VacancyUser user)
        {
            var validationRules = VacancyRuleSet.EmployerNameOption;

            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _providerVacancyClient, _recruitVacancyClient, model, RouteNames.EmployerName_Post);
            
            vacancy.EmployerNameOption =  model.SelectedEmployerNameOption.HasValue 
                ? model.SelectedEmployerNameOption.Value.ConvertToDomainOption()
                : (EmployerNameOption?) null;

            // temporarily set the employer name for validation
            if (model.SelectedEmployerNameOption == EmployerNameOptionViewModel.NewTradingName)
            {
                validationRules = VacancyRuleSet.EmployerNameOption | VacancyRuleSet.TradingName;
                vacancy.EmployerName = model.NewTradingName;
            }

            return await ValidateAndExecute(
                vacancy, 
                v => _recruitVacancyClient.Validate(v, validationRules),
                v => Task.FromResult(new OrchestratorResponse(true)));
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, EmployerNameEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, EmployerNameEditModel>();

            mappings.Add(v => v.EmployerName, vm => vm.NewTradingName);
            mappings.Add(v => v.EmployerNameOption, vm => vm.SelectedEmployerNameOption);

            return mappings;
        }
        
    }
}