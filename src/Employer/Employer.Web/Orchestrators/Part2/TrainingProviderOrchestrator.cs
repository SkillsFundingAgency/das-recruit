using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class TrainingProviderOrchestrator : EntityValidatingOrchestrator<Vacancy, ConfirmTrainingProviderEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProvider;
        private readonly IEmployerVacancyClient _client;
        private readonly ITrainingProviderService _providerService;
        private readonly IReviewSummaryService _reviewSummaryService;

        public TrainingProviderOrchestrator(IEmployerVacancyClient client, ITrainingProviderService providerService, ILogger<TrainingProviderOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _providerService = providerService;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<SelectTrainingProviderViewModel> GetSelectTrainingProviderViewModel(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, vrm, RouteNames.TrainingProvider_Select_Get);
            
            var vm = new SelectTrainingProviderViewModel
            {
                Title = vacancy.Title,
                Ukprn = vacancy.TrainingProvider?.Ukprn?.ToString()
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModel(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetTrainingProviderFieldIndicators());
            }

            return vm;
        }

        public async Task<SelectTrainingProviderViewModel> GetSelectTrainingProviderViewModel(SelectTrainingProviderEditModel m)
        {
            var vm = await GetSelectTrainingProviderViewModel((VacancyRouteModel)m);
            vm.Ukprn = m.Ukprn;
            return vm;
        }

        public async Task<ConfirmTrainingProviderViewModel> GetConfirmViewModel(SelectTrainingProviderEditModel m)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, m, RouteNames.TrainingProvider_Confirm_Get);
            
            if (long.TryParse(m.Ukprn, out var ukprn) && ukprn != vacancy.TrainingProvider?.Ukprn)
            {
                var provider = await _providerService.GetProviderAsync(ukprn);

                return new ConfirmTrainingProviderViewModel
                {
                    EmployerAccountId = m.EmployerAccountId,
                    VacancyId = m.VacancyId,
                    Title = vacancy.Title,
                    Ukprn = provider.Ukprn.Value,
                    ProviderName = provider.Name,
                    ProviderAddress = provider.Address.GetInlineAddress()
                };
            }

            return new ConfirmTrainingProviderViewModel
            {
                EmployerAccountId = m.EmployerAccountId,
                VacancyId = m.VacancyId,
                Title = vacancy.Title,
                Ukprn = vacancy.TrainingProvider.Ukprn.Value,
                ProviderName = vacancy.TrainingProvider.Name,
                ProviderAddress = vacancy.TrainingProvider.Address.GetInlineAddress()
            };
        }

        public Task<OrchestratorResponse> PostConfirmEditModelAsync(ConfirmTrainingProviderEditModel m, VacancyUser user)
        {
            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, m, RouteNames.TrainingProvider_Confirm_Post);
            var providerTask = _providerService.GetProviderAsync(long.Parse(m.Ukprn));

            Task.WaitAll(vacancyTask, providerTask);

            var vacancy = vacancyTask.Result;
            var provider = providerTask.Result;
            
            vacancy.TrainingProvider = provider;

            return ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        public Task<bool> ConfirmProviderExists(long ukprn)
        {
            return _providerService.ExistsAsync(ukprn);
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ConfirmTrainingProviderEditModel> DefineMappings()
        {
            return new EntityToViewModelPropertyMappings<Vacancy, ConfirmTrainingProviderEditModel>
            {
                { e => e.TrainingProvider.Ukprn, vm => vm.Ukprn }
            };
        }
    }
}