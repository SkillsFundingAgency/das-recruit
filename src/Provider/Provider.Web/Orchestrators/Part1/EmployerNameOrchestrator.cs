using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1;

public class EmployerNameOrchestrator(
    IProviderVacancyClient providerVacancyClient,
    IRecruitVacancyClient recruitVacancyClient,
    ILogger<EmployerNameOrchestrator> logger,
    IReviewSummaryService reviewSummaryService,
    IUtility utility)
    : EntityValidatingOrchestrator<Vacancy, EmployerNameEditModel>(logger)
{
    private Expression<Func<EmployerNameEditModel, object>> _vmPropertyToMapEmployerNameTo = null;

    public async Task<EmployerNameViewModel> GetEmployerNameViewModelAsync(
        TaskListViewModel vrm,
        VacancyEmployerInfoModel employerInfoModel,
        VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.EmployerName_Get);
        var accountLegalEntityPublicHashedId = employerInfoModel?.AccountLegalEntityPublicHashedId ?? vacancy.AccountLegalEntityPublicHashedId;
        var getVacancyEditInfoTask = providerVacancyClient.GetProviderEditVacancyInfoAsync(vrm.Ukprn);
        var getEmployerProfileTask = recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, accountLegalEntityPublicHashedId);

        await Task.WhenAll(getVacancyEditInfoTask, getEmployerProfileTask);

        var employerInfo = getVacancyEditInfoTask.Result.Employers.Single(e => e.EmployerAccountId == vacancy.EmployerAccountId);
        var employerProfile = getEmployerProfileTask.Result;
        var legalEntity = employerInfo.LegalEntities.Single(l => l.AccountLegalEntityPublicHashedId == accountLegalEntityPublicHashedId);

        var vm = new EmployerNameViewModel 
        {
            AnonymousName = employerInfoModel?.AnonymousName ,
            AnonymousReason = employerInfoModel?.AnonymousReason ?? vacancy.AnonymousReason,
            ExistingTradingName = employerProfile?.TradingName,
            HasOnlyOneOrganisation = employerInfo.LegalEntities.Count == 1,
            IsTaskList = vrm.IsTaskList,
            IsTaskListCompleted = utility.IsTaskListCompleted(vacancy) && !vrm.IsTaskList,
            LegalEntityName = legalEntity.Name,
            NewTradingName = employerInfoModel?.NewTradingName,
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            SelectedEmployerIdentityOption = employerInfoModel?.EmployerIdentityOption ?? vacancy?.EmployerNameOption?.ConvertToModelOption(),      
            Title = vacancy.Title,
            Ukprn = vrm.Ukprn,
            VacancyId = vrm.VacancyId,
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

        // temporarily set the employer name for validation
        EmployerProfile profile = null;
        if (model.SelectedEmployerIdentityOption == EmployerIdentityOption.NewTradingName)
        {
            validationRules = VacancyRuleSet.EmployerNameOption | VacancyRuleSet.TradingName;
            vacancy.EmployerName = model.NewTradingName;
            _vmPropertyToMapEmployerNameTo = vm => vm.NewTradingName;
            profile = await recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId,
                vacancy.AccountLegalEntityPublicHashedId);
        }

        if (model.SelectedEmployerIdentityOption == EmployerIdentityOption.Anonymous)
        {
            vacancy.EmployerName = model.AnonymousName;
            vacancy.AnonymousReason = model.AnonymousReason;
            _vmPropertyToMapEmployerNameTo = vm => vm.AnonymousName;
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
                return Task.FromResult(new OrchestratorResponse(true));
            });
    }

    protected override EntityToViewModelPropertyMappings<Vacancy, EmployerNameEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, EmployerNameEditModel>();

        if (_vmPropertyToMapEmployerNameTo != null)
            mappings.Add(v => v.EmployerName, _vmPropertyToMapEmployerNameTo);

        mappings.Add(v => v.EmployerNameOption, vm => vm.SelectedEmployerIdentityOption);

        return mappings;
    }
}