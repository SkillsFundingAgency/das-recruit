using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;
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

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class TrainingProviderOrchestrator : VacancyValidatingOrchestrator<ConfirmTrainingProviderEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProvider;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly ITrainingProviderSummaryProvider _trainingProviderSummaryProvider;
        private readonly ITrainingProviderService _trainingProviderService;

        public TrainingProviderOrchestrator(
            IEmployerVacancyClient client,
            IRecruitVacancyClient vacancyClient,
            ILogger<TrainingProviderOrchestrator> logger,
            IReviewSummaryService reviewSummaryService,
            ITrainingProviderSummaryProvider trainingProviderSummarayProvider,
            ITrainingProviderService trainingProviderService
            ) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _trainingProviderSummaryProvider = trainingProviderSummarayProvider;
            _trainingProviderService = trainingProviderService;
        }

        public async Task<SelectTrainingProviderViewModel> GetSelectTrainingProviderViewModelAsync(VacancyRouteModel vrm, long? ukprn = null)
        {
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.TrainingProvider_Select_Get);
            var trainingProvidersTask = _trainingProviderSummaryProvider.FindAllAsync();

            await Task.WhenAll(vacancyTask, trainingProvidersTask);

            var vacancy = vacancyTask.Result;
            var trainingProviders = trainingProvidersTask.Result;

            var programme = await _vacancyClient.GetApprenticeshipProgrammeAsync(vacancy.ProgrammeId);

            var vm = new SelectTrainingProviderViewModel
            {
                Title = vacancy.Title,
                TrainingProviders = trainingProviders.Select(t => FormatSuggestion(t.ProviderName, t.Ukprn)),
                PageInfo = Utility.GetPartOnePageInfo(vacancy),
                Programme = programme.ToViewModel()
            };

            TrySetSelectedTrainingProvider(vm, trainingProviders, vacancy, ukprn);
            
            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetTrainingProviderFieldIndicators());
            }

            return vm;
        }

        public async Task<OrchestratorResponse<PostSelectTrainingProviderResult>> PostSelectTrainingProviderAsync(SelectTrainingProviderEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.TrainingProvider_Select_Post);

            if (m.IsTrainingProviderSelected == false)
            {
                if (vacancy.TrainingProvider != null)
                {
                    vacancy.TrainingProvider = null;
                    await _vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                }

                return new OrchestratorResponse<PostSelectTrainingProviderResult>(new PostSelectTrainingProviderResult());
            }

            var providerSummary = await GetProviderFromModelAsync(m);

            TrainingProvider provider = null;
            if (providerSummary != null)
                provider = await _trainingProviderService.GetProviderAsync(providerSummary.Ukprn);

            vacancy.TrainingProvider = provider;

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
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
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.TrainingProvider_Confirm_Get);
            var providerTask = _trainingProviderService.GetProviderAsync(ukprn);

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
                PageInfo = Utility.GetPartOnePageInfo(vacancy)
            };
        }

        public async Task<OrchestratorResponse> PostConfirmEditModelAsync(ConfirmTrainingProviderEditModel m, VacancyUser user)
        {
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.TrainingProvider_Confirm_Post);
            var providerTask = _trainingProviderService.GetProviderAsync(long.Parse(m.Ukprn));

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
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        public async Task<TrainingProviderSummary> GetProviderAsync(string ukprn)
        {
            if (long.TryParse(ukprn, out var ukprnAsLong) == false)
                return null;

            return await _trainingProviderSummaryProvider.GetAsync(ukprnAsLong);
        }

        private async Task<TrainingProviderSummary> GetProviderFromModelAsync(SelectTrainingProviderEditModel model)
        {
            if (model.SelectionType == TrainingProviderSelectionType.TrainingProviderSearch)
            {
                if (model.TrainingProviderSearch.EndsWith(EsfaTestTrainingProvider.Ukprn.ToString()))
                    return await GetProviderAsync(EsfaTestTrainingProvider.Ukprn.ToString());

                var allProviders = await _trainingProviderSummaryProvider.FindAllAsync();

                var matches = allProviders.Where(p =>
                    FormatSuggestion(p.ProviderName, p.Ukprn).Contains(model.TrainingProviderSearch))
                    .ToList();

                return matches.Count() == 1 ? matches.First() : null;
            }

            return await GetProviderAsync(model.Ukprn);
        }

        private void TrySetSelectedTrainingProvider(SelectTrainingProviderViewModel vm, IEnumerable<TrainingProviderSummary> trainingProviders, Vacancy vacancy, long? ukprn)
        {
            if (ukprn.HasValue)
            {
                SetModelUsingUkprn(vm, trainingProviders, ukprn.Value);
                return;
            }

            if (vacancy.TrainingProvider != null)
                SetModelUsingVacancyTrainingProvider(vm, vacancy);
        }
        
        private void SetModelUsingVacancyTrainingProvider(SelectTrainingProviderViewModel vm, Vacancy vacancy)
        {
            vm.Ukprn = vacancy.TrainingProvider.Ukprn.ToString();
            vm.TrainingProviderSearch = FormatSuggestion(vacancy.TrainingProvider.Name, vacancy.TrainingProvider.Ukprn.Value);
            vm.IsTrainingProviderSelected = true;
        }

        private void SetModelUsingUkprn(SelectTrainingProviderViewModel vm, IEnumerable<TrainingProviderSummary> trainingProviders, long ukprn)
        {
            var trainingProvider = trainingProviders.SingleOrDefault(p => p.Ukprn == ukprn);
            if (trainingProvider == null)
                return;

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

        private string FormatSuggestion(string providerName, long ukprn)
        {
            return $"{providerName.ToUpper()} {ukprn}";
        }
    }
}