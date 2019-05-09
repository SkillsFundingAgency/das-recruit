using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Models;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class EmployerNameOrchestrator : EntityValidatingOrchestrator<Vacancy, EmployerNameEditModel>
    {
        private readonly IEmployerVacancyClient _employerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        private Expression<Func<EmployerNameEditModel, object>> _vmPropertyToMapEmployerNameTo = null;

        public EmployerNameOrchestrator(IEmployerVacancyClient employerVacancyClient, IRecruitVacancyClient recruitVacancyClient, 
            ILogger<EmployerNameOrchestrator> logger, IReviewSummaryService reviewSummaryService)
            : base(logger) 
        {
            _employerVacancyClient = employerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<EmployerNameViewModel> GetEmployerNameViewModelAsync(
            VacancyRouteModel vrm, VacancyEmployerInfoModel employerInfoModel, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _employerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Employer_Get);

            var legalEntityId = employerInfoModel.LegalEntityId.GetValueOrDefault();
                
            var getEmployerDataTask = _employerVacancyClient.GetEditVacancyInfoAsync(vrm.EmployerAccountId);
            var getEmployerProfileTask = _recruitVacancyClient.GetEmployerProfileAsync(vrm.EmployerAccountId, legalEntityId);
            await Task.WhenAll(getEmployerDataTask, getEmployerProfileTask);
            var editVacancyInfo = getEmployerDataTask.Result;
            var employerProfile = getEmployerProfileTask.Result;

            var legalEntity = editVacancyInfo.LegalEntities.Single(l => l.LegalEntityId == legalEntityId);

            var vm = new EmployerNameViewModel 
            {
                HasOnlyOneOrganisation = editVacancyInfo.LegalEntities.Count() == 1,
                LegalEntityName = legalEntity.Name,
                ExistingTradingName = employerProfile.TradingName,
                PageInfo = Utility.GetPartOnePageInfo(vacancy),
                SelectedEmployerIdentityOption = employerInfoModel.EmployerIdentityOption,
                NewTradingName = employerInfoModel.NewTradingName,
                AnonymousName = employerInfoModel.AnonymousName,
                AnonymousReason = employerInfoModel.AnonymousReason
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
                _employerVacancyClient, _recruitVacancyClient, model, RouteNames.EmployerName_Post);
            
            vacancy.EmployerNameOption =  model.SelectedEmployerIdentityOption.HasValue 
                ? model.SelectedEmployerIdentityOption.Value.ConvertToDomainOption()
                : (EmployerNameOption?) null;

            // temporarily set the employer name for validation
            if (model.SelectedEmployerIdentityOption == EmployerIdentityOption.NewTradingName)
            {
                validationRules = VacancyRuleSet.EmployerNameOption | VacancyRuleSet.TradingName;
                vacancy.EmployerName = model.NewTradingName;
                _vmPropertyToMapEmployerNameTo = vm => vm.NewTradingName;
            }

            if (model.SelectedEmployerIdentityOption == EmployerIdentityOption.Anonymous)
            {
                vacancy.EmployerName = model.AnonymousName;
                vacancy.AnonymousReason = model.AnonymousReason;
                _vmPropertyToMapEmployerNameTo = vm => vm.AnonymousName;
            }

            return await ValidateAndExecute(
                vacancy, 
                v => _recruitVacancyClient.Validate(v, validationRules),
                v => Task.FromResult(new OrchestratorResponse(true)));
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, EmployerNameEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, EmployerNameEditModel>();

            if(_vmPropertyToMapEmployerNameTo != null)
                mappings.Add(v => v.EmployerName, _vmPropertyToMapEmployerNameTo);

            mappings.Add(v => v.EmployerNameOption, vm => vm.SelectedEmployerIdentityOption);

            return mappings;
        }
    }    
}