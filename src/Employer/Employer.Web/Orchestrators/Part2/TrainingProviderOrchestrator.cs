using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.TrainingProvider;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public enum PostSelectTrainingProviderResultAction
    {
        TrainingProviderNotFound,
        TrainingProviderContinue,
        TrainingProviderConfirm
    }

    public class PostSelectTrainingProviderResult
    {
        public long? FoundProviderUkprn { get; set; }

        public PostSelectTrainingProviderResultAction Action { get; set; }
    }

    public class TrainingProviderOrchestrator : EntityValidatingOrchestrator<Vacancy, ConfirmTrainingProviderEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProvider;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly ICache _cache;
        private readonly ITimeProvider _timeProvider;
        private readonly ExternalLinksConfiguration _externalLinks;

        public TrainingProviderOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<TrainingProviderOrchestrator> logger, IReviewSummaryService reviewSummaryService, ICache cache, ITimeProvider timeProvider, IOptions<ExternalLinksConfiguration> externalLinks) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _cache = cache;
            _timeProvider = timeProvider;
            _externalLinks = externalLinks.Value;
        }

        public async Task<SelectTrainingProviderViewModel> GetSelectTrainingProviderViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.TrainingProvider_Select_Get);
            var trainingProvidersTask = GetAllTrainingProvidersAsync();

            await Task.WhenAll(vacancyTask, trainingProvidersTask);

            var vacancy = vacancyTask.Result;
            
            var vm = new SelectTrainingProviderViewModel
            {
                Title = vacancy.Title,
                Ukprn = vacancy.TrainingProvider?.Ukprn?.ToString(),
                FindProviderUrl = _externalLinks.FindProviderUrl,
                TrainingProviderSearch = vacancy.TrainingProvider != null ? FormatSuggestion(vacancy.TrainingProvider.Name, vacancy.TrainingProvider.Ukprn.Value) : null,
                TrainingProviders = trainingProvidersTask.Result.Select(t => FormatSuggestion(t.ProviderName, t.Ukprn)),
                SelectTrainingProvider = vacancy.TrainingProvider != null ? true : (bool?)null 
            };
            
            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetTrainingProviderFieldIndicators());
            }

            return vm;
        }

        public async Task<PostSelectTrainingProviderResult> PostSelectTrainingProviderAsync(SelectTrainingProviderEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.TrainingProvider_Select_Post);

            if (m.SelectTrainingProvider == false)
            {
                if (vacancy.TrainingProvider != null)
                {
                    vacancy.TrainingProvider = null;
                    await _vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                }
                
                return new PostSelectTrainingProviderResult { Action = PostSelectTrainingProviderResultAction.TrainingProviderContinue };
            }
        
            var provider = await GetProviderFromModelAsync(m);

            if (provider == null)
                return new PostSelectTrainingProviderResult { Action = PostSelectTrainingProviderResultAction.TrainingProviderNotFound };

            return new PostSelectTrainingProviderResult
            {
                Action = PostSelectTrainingProviderResultAction.TrainingProviderConfirm,
                FoundProviderUkprn = provider.Ukprn
            };
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
            var providerTask = _client.GetTrainingProviderAsync(ukprn);

            await Task.WhenAll(vacancyTask, providerTask);

            var vacancy = vacancyTask.Result;
            var provider = providerTask.Result;

            return new ConfirmTrainingProviderViewModel {
                EmployerAccountId = vrm.EmployerAccountId,
                VacancyId = vrm.VacancyId,
                Title = vacancy.Title,
                Ukprn = provider.Ukprn.Value,
                ProviderName = provider.Name,
                ProviderAddress = provider.Address.ToAddressString()
            };
        }

        public async Task<OrchestratorResponse> PostConfirmEditModelAsync(ConfirmTrainingProviderEditModel m, VacancyUser user)
        {
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.TrainingProvider_Confirm_Post);
            var providerTask = _client.GetTrainingProviderAsync(long.Parse(m.Ukprn));

            await Task.WhenAll(vacancyTask, providerTask);

            var vacancy = vacancyTask.Result;
            var provider = providerTask.Result;

            vacancy.TrainingProvider = provider;

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        private async Task<TrainingProviderSuggestion> GetProviderFromModelAsync(SelectTrainingProviderEditModel model)
        {
            if (model.SelectionType == TrainingProviderSelectionType.TrainingProviderSearch)
            {
                var allProviders = await GetAllTrainingProvidersAsync();

                var matches = allProviders.Where(p => 
                    FormatSuggestion(p.ProviderName, p.Ukprn).Contains(model.TrainingProviderSearch))
                    .ToList();

                return matches.Count() == 1 ? matches.First() : null;
            }

            return await GetProviderAsync(model.Ukprn);
        }

        public async Task<TrainingProviderSuggestion> GetProviderAsync(string ukprn)
        {
            if (long.TryParse(ukprn, out var ukprnAsLong) == false)
                return null;
            
            var allProviders = await GetAllTrainingProvidersAsync();
            return allProviders.SingleOrDefault(p => p.Ukprn == ukprnAsLong);
        }

        private Task<IEnumerable<TrainingProviderSuggestion>> GetAllTrainingProvidersAsync()
        {
            return _cache.CacheAsideAsync(
                CacheKeys.TrainingProviders,
                _timeProvider.NextDay6am,
                () => _client.GetAllTrainingProviders());
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