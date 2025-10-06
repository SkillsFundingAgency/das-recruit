using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1;

public class EmployerNameOrchestrator(
    IEmployerVacancyClient employerVacancyClient,
    IRecruitVacancyClient recruitVacancyClient,
    ILogger<EmployerNameOrchestrator> logger,
    IReviewSummaryService reviewSummaryService,
    IUtility utility)
    : VacancyValidatingOrchestrator<EmployerNameEditModel>(logger)
{
    private Expression<Func<EmployerNameEditModel, object>> _vmPropertyToMapEmployerNameTo = null;

    public async Task<EmployerNameViewModel> GetEmployerNameViewModelAsync(TaskListViewModel vrm, VacancyEmployerInfoModel employerInfoModel)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm);
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
            AnonymousName = employerInfoModel?.AnonymousName ,
            AnonymousReason = employerInfoModel?.AnonymousReason ?? vacancy?.AnonymousReason,
            EmployerAccountId = vrm.EmployerAccountId,
            ExistingTradingName = employerProfile.TradingName,
            HasOnlyOneOrganisation = editVacancyInfo.LegalEntities.Count() == 1,
            IsTaskList = vrm.IsTaskList,
            LegalEntityName = legalEntity.Name,
            NewTradingName = employerInfoModel?.NewTradingName,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            SelectedEmployerIdentityOption = employerInfoModel?.EmployerIdentityOption ?? vacancy?.EmployerNameOption?.ConvertToModelOption(),
            TaskListCompleted = utility.IsTaskListCompleted(vacancy) && !vrm.IsTaskList,
            VacancyId = vrm.VacancyId,
            VacancyTitle = vacancy!.Title,
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
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model);
        vacancy.EmployerNameOption =  model.SelectedEmployerIdentityOption?.ConvertToDomainOption();

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.EmployerNameOption,
            FieldIdResolver.ToFieldId(v => v.EmployerName),
            vacancy,
            _ => model.SelectedEmployerIdentityOption?.ConvertToDomainOption());
            
        // temporarily set the employer name for validation
        EmployerProfile profile = null;
        if (model.SelectedEmployerIdentityOption == EmployerIdentityOption.NewTradingName)
        {
            profile = await recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);

            SetVacancyWithEmployerReviewFieldIndicators(
                profile.TradingName,
                FieldIdResolver.ToFieldId(v => v.EmployerName),
                vacancy,
                _ => model.NewTradingName);
                
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
            vacancy.AnonymousReason = null;
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