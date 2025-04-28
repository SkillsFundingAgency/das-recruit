using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
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
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using FeatureNames = Esfa.Recruit.Employer.Web.Configuration.FeatureNames;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class EmployerNameOrchestrator(
        IEmployerVacancyClient employerVacancyClient,
        IRecruitVacancyClient recruitVacancyClient,
        ILogger<EmployerNameOrchestrator> logger,
        IReviewSummaryService reviewSummaryService,
        IUtility utility,
        IFeature feature)
        : VacancyValidatingOrchestrator<EmployerNameEditModel>(logger)
    {
        private Expression<Func<EmployerNameEditModel, object>> _vmPropertyToMapEmployerNameTo = null;

        public async Task<EmployerNameViewModel> GetEmployerNameViewModelAsync(VacancyRouteModel vrm, VacancyEmployerInfoModel employerInfoModel)
        {
            var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Employer_Get);

            var accountLegalEntityPublicHashedId = employerInfoModel?.AccountLegalEntityPublicHashedId ?? vacancy.AccountLegalEntityPublicHashedId;

            if (string.IsNullOrEmpty(accountLegalEntityPublicHashedId))
            {
                return null;
            }
                
            var getEmployerDataTask = employerVacancyClient.GetEditVacancyInfoAsync(vrm.EmployerAccountId);
            var getEmployerProfileTask = recruitVacancyClient.GetEmployerProfileAsync(vrm.EmployerAccountId, accountLegalEntityPublicHashedId);
            await Task.WhenAll(getEmployerDataTask, getEmployerProfileTask);
            var editVacancyInfo = getEmployerDataTask.Result;
            var employerProfile = getEmployerProfileTask.Result;

            var legalEntity = editVacancyInfo.LegalEntities.Single(l => l.AccountLegalEntityPublicHashedId == accountLegalEntityPublicHashedId);

            var vm = new EmployerNameViewModel 
            {
                VacancyId = vrm.VacancyId,
                EmployerAccountId = vrm.EmployerAccountId,
                HasOnlyOneOrganisation = editVacancyInfo.LegalEntities.Count() == 1,
                LegalEntityName = legalEntity.Name,
                ExistingTradingName = employerProfile.TradingName,
                PageInfo = utility.GetPartOnePageInfo(vacancy),
                SelectedEmployerIdentityOption = employerInfoModel?.EmployerIdentityOption ?? vacancy?.EmployerNameOption?.ConvertToModelOption(),
                NewTradingName = employerInfoModel?.NewTradingName,
                AnonymousName = employerInfoModel?.AnonymousName ,
                AnonymousReason = employerInfoModel?.AnonymousReason ?? vacancy?.AnonymousReason,
                TaskListCompleted = utility.IsTaskListCompleted(vacancy)
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value, 
                    ReviewFieldMappingLookups.GetEmployerNameReviewFieldIndicators());
            }

            return vm;
        }

        public async Task<OrchestratorResponse> PostEmployerNameEditModelAsync(
            EmployerNameEditModel model, VacancyUser user)
        {
            var validationRules = VacancyRuleSet.EmployerNameOption;
            var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.EmployerName_Post);
            vacancy.EmployerNameOption =  model.SelectedEmployerIdentityOption?.ConvertToDomainOption();

            if (feature.IsFeatureEnabled(FeatureNames.MultipleLocations))
            {
                SetVacancyWithEmployerReviewFieldIndicators(
                    vacancy.EmployerNameOption,
                    FieldIdResolver.ToFieldId(v => v.EmployerName),
                    vacancy,
                    _ => model.SelectedEmployerIdentityOption?.ConvertToDomainOption());
            }
            
            // temporarily set the employer name for validation
            EmployerProfile profile = null;
            if (model.SelectedEmployerIdentityOption == EmployerIdentityOption.NewTradingName)
            {
                profile = await recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);

                if (feature.IsFeatureEnabled(FeatureNames.MultipleLocations))
                {
                    SetVacancyWithEmployerReviewFieldIndicators(
                        profile.TradingName,
                        FieldIdResolver.ToFieldId(v => v.EmployerName),
                        vacancy,
                        _ => model.NewTradingName);
                }
                
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
            else
            {
                if (feature.IsFeatureEnabled(FeatureNames.MultipleLocations))
                {
                    vacancy.AnonymousReason = null;
                }
            }
            
            return await ValidateAndExecute(
                vacancy, 
                v => recruitVacancyClient.Validate(v, validationRules),
                async v =>
                {
                    await recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                    if (profile != null)
                    {
                        await utility.UpdateEmployerProfile(new VacancyEmployerInfoModel
                        {
                            NewTradingName = model.NewTradingName,
                            EmployerIdentityOption = EmployerIdentityOption.NewTradingName
                        }, profile, null, user);
                    }
                });
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, EmployerNameEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, EmployerNameEditModel>();
            if (_vmPropertyToMapEmployerNameTo != null)
            {
                mappings.Add(v => v.EmployerName, _vmPropertyToMapEmployerNameTo);
            }

            mappings.Add(v => v.EmployerNameOption, vm => vm.SelectedEmployerIdentityOption);
            return mappings;
        }
    }    
}