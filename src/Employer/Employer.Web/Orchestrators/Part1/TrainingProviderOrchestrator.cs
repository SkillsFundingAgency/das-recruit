using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1;

public class TrainingProviderOrchestrator(
    IRecruitVacancyClient vacancyClient,
    ILogger<TrainingProviderOrchestrator> logger,
    IReviewSummaryService reviewSummaryService,
    ITrainingProviderSummaryProvider trainingProviderSummaryProvider,
    ITrainingProviderService trainingProviderService,
    IUtility utility,
    RecruitConfiguration recruitConfiguration,
    ITaskListValidator taskListValidator)
    : VacancyValidatingOrchestrator<ConfirmTrainingProviderEditModel>(logger)
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProvider;

    public virtual async Task<SelectTrainingProviderViewModel> GetSelectTrainingProviderViewModelAsync(VacancyRouteModel vrm, long? ukprn = null)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm);

        var includePlaceholderProgramme = vacancy.EmployerAccountId.Equals(recruitConfiguration.EmployerAccountId,
            StringComparison.CurrentCultureIgnoreCase);
        var programme = await vacancyClient.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId, includePlaceholderProgramme);
        
        // TODO: temporarily commented out as part of FAI-2818
        // var trainingProviders = int.TryParse(programme.Id, out int programmeId)
        //     ? (await trainingProviderService.GetCourseProviders(programmeId)).ToList()
        //     : (await trainingProviderSummaryProvider.FindAllAsync()).ToList();
        
        var trainingProviders = (await trainingProviderSummaryProvider.FindAllAsync()).ToList();

        var vm = new SelectTrainingProviderViewModel
        {
            VacancyId = vrm.VacancyId,
            EmployerAccountId = vrm.EmployerAccountId,
            Title = vacancy.Title,
            TrainingProviders = trainingProviders.Select(t => FormatSuggestion(t.ProviderName, t.Ukprn)),
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            Programme = programme.ToViewModel(),
            CurrentSelectedTrainingProvider = vacancy.TrainingProvider is not null
                ? FormatSuggestion(vacancy.TrainingProvider.Name, vacancy.TrainingProvider.Ukprn!.Value)
                : null
        };

        TrySetSelectedTrainingProvider(vm, trainingProviders, vacancy, ukprn);
            
        if (vacancy.Status == VacancyStatus.Referred)
        {
            vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                ReviewFieldMappingLookups.GetTrainingProviderFieldIndicators());
        }

        return vm;
    }

    public async Task<OrchestratorResponse<PostSelectTrainingProviderResult>> PostSelectTrainingProviderAsync(SelectTrainingProviderEditModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m);
        var providerSummary = await GetProviderFromModelAsync(m, vacancy.EmployerAccountId, vacancy.ProgrammeId);

        TrainingProvider provider = null;
        if (providerSummary != null)
            provider = await trainingProviderService.GetProviderAsync(providerSummary.Ukprn);

        vacancy.TrainingProvider = provider;

        return await ValidateAndExecute(
            vacancy,
            v => vacancyClient.Validate(v, ValidationRules),
            v => Task.FromResult(new PostSelectTrainingProviderResult
            {
                FoundTrainingProviderUkprn = v.TrainingProvider?.Ukprn
            })
        );
    }

    public async Task<SelectTrainingProviderViewModel> GetSelectTrainingProviderViewModelAsync(SelectTrainingProviderEditModel m)
    {
        var vm = await GetSelectTrainingProviderViewModelAsync((VacancyRouteModel)m);
        vm.Ukprn = m.Ukprn;
        vm.TrainingProviderSearch = m.TrainingProviderSearch;
        return vm;
    }

    public async Task<ConfirmTrainingProviderViewModel> GetConfirmViewModelAsync(VacancyRouteModel vrm, long ukprn)
    {
        var vacancyTask = utility.GetAuthorisedVacancyForEditAsync(vrm);
        var providerTask = trainingProviderService.GetProviderAsync(ukprn);

        await Task.WhenAll(vacancyTask, providerTask);

        var vacancy = vacancyTask.Result;
        var provider = providerTask.Result;

        return new ConfirmTrainingProviderViewModel
        {
            EmployerAccountId = vrm.EmployerAccountId,
            VacancyId = vrm.VacancyId,
            Title = vacancy.Title,
            Ukprn = provider.Ukprn.Value,
            ProviderName = provider.Name,
            ProviderAddress = provider.Address.ToAddressString(),
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            WillTaskListBeCompleted = await taskListValidator.IsCompleteAsync(VacancyWithProposedChanges(vacancy, provider), EmployerTaskListSectionFlags.All)
        };
    }
        
    private static Vacancy VacancyWithProposedChanges(Vacancy vacancy, TrainingProvider provider)
    {
        // we need to set the vacancy up as if it had the proposed changes so that the tasklist validator can work correctly
        // clone the vacancy to make sure nothing accidentally gets saved
        var clone = JsonConvert.DeserializeObject<Vacancy>(JsonConvert.SerializeObject(vacancy));
        clone.TrainingProvider = provider;
        return clone;
    }

    public async Task<OrchestratorResponse> PostConfirmEditModelAsync(ConfirmTrainingProviderEditModel m, VacancyUser user)
    {
        var vacancyTask = utility.GetAuthorisedVacancyForEditAsync(m);
        var providerTask = trainingProviderService.GetProviderAsync(long.Parse(m.Ukprn));
        await Task.WhenAll(vacancyTask, providerTask);
        var vacancy = vacancyTask.Result;
        var provider = providerTask.Result;

        // this has diverged from the usual pattern because only a single individual property is a review field
        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.TrainingProvider?.Ukprn,
            FieldIdResolver.ToFieldId(v => v.TrainingProvider.Ukprn),
            vacancy,
            (v) =>
            {
                return provider.Ukprn;
            });

        vacancy.TrainingProvider = provider;

        return await ValidateAndExecute(
            vacancy,
            v => vacancyClient.Validate(v, ValidationRules),
            v => vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
        );
    }

    public async Task<TrainingProviderSummary> GetProviderAsync(string ukprn)
    {
        if (long.TryParse(ukprn, out var ukprnAsLong) == false)
            return null;

        return await trainingProviderSummaryProvider.GetAsync(ukprnAsLong);
    }

    private async Task<TrainingProviderSummary> GetProviderFromModelAsync(SelectTrainingProviderEditModel model,
        string employerAccountId, string programmeId)
    {
        if (model.SelectionType == TrainingProviderSelectionType.TrainingProviderSearch)
        {
            if (employerAccountId.Equals(recruitConfiguration.EmployerAccountId, StringComparison.CurrentCultureIgnoreCase) 
                && model.TrainingProviderSearch.EndsWith(EsfaTestTrainingProvider.Ukprn.ToString()))
                return await GetProviderAsync(EsfaTestTrainingProvider.Ukprn.ToString());

            // TODO: Temporarily commented out for FAI-2818
            // var trainingProviders = int.TryParse(programmeId, out int id)
            //     ? (await trainingProviderService.GetCourseProviders(id)).ToList()
            //     : (await trainingProviderSummaryProvider.FindAllAsync()).ToList();

            var trainingProviders = await trainingProviderSummaryProvider.FindAllAsync();

            var matches = trainingProviders
                .Where(p => FormatSuggestion(p.ProviderName, p.Ukprn).Contains(model.TrainingProviderSearch))
                .ToList();

            return matches.Count == 1 ? matches.First() : null;
        }

        return await GetProviderAsync(model.Ukprn);
    }

    private static void TrySetSelectedTrainingProvider(SelectTrainingProviderViewModel vm, IEnumerable<TrainingProviderSummary> trainingProviders, Vacancy vacancy, long? ukprn)
    {
        if (ukprn.HasValue)
        {
            SetModelUsingUkprn(vm, trainingProviders, ukprn.Value);
            return;
        }

        if (vacancy.TrainingProvider != null)
        {
            SetModelUsingVacancyTrainingProvider(vm, trainingProviders, vacancy);
        }
    }
        
    private static void SetModelUsingVacancyTrainingProvider(SelectTrainingProviderViewModel vm,
        IEnumerable<TrainingProviderSummary> trainingProviders, Vacancy vacancy)
    {
        // TODO: Temporarily commented out for FAI-2818
        // if (trainingProviders.SingleOrDefault(x => x.ProviderName == vacancy.TrainingProvider.Name) is null)
        // {
        //     vm.ProviderDoesNotSupportCourse = true;
        //     return;
        // }
        
        vm.Ukprn = vacancy.TrainingProvider.Ukprn.ToString();
        vm.TrainingProviderSearch = FormatSuggestion(vacancy.TrainingProvider.Name, vacancy.TrainingProvider.Ukprn.Value);
        vm.IsTrainingProviderSelected = true;
    }

    private static void SetModelUsingUkprn(SelectTrainingProviderViewModel vm, IEnumerable<TrainingProviderSummary> trainingProviders, long ukprn)
    {
        var trainingProvider = trainingProviders.SingleOrDefault(p => p.Ukprn == ukprn);
        if (trainingProvider == null)
        {
            vm.ProviderDoesNotSupportCourse = true;
            return;
        }

        vm.Ukprn = ukprn.ToString();
        vm.TrainingProviderSearch = FormatSuggestion(trainingProvider.ProviderName, trainingProvider.Ukprn);
        vm.IsTrainingProviderSelected = true;
    }

    protected override EntityToViewModelPropertyMappings<Vacancy, ConfirmTrainingProviderEditModel> DefineMappings()
    {
        return new EntityToViewModelPropertyMappings<Vacancy, ConfirmTrainingProviderEditModel>
        {
            { e => e.TrainingProvider.Ukprn, vm => vm.Ukprn }
        };
    }

    private static string FormatSuggestion(string providerName, long ukprn)
    {
        return $"{providerName.ToUpper()} ({ukprn})";
    }
}