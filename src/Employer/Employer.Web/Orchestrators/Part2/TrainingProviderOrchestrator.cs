using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class TrainingProviderOrchestrator : EntityValidatingOrchestrator<Vacancy, ConfirmTrainingProviderEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProvider;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public TrainingProviderOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<TrainingProviderOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<SelectTrainingProviderViewModel> GetSelectTrainingProviderViewModel(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.TrainingProvider_Select_Get);
            
            var vm = new SelectTrainingProviderViewModel
            {
                Title = vacancy.Title,
                Ukprn = vacancy.TrainingProvider?.Ukprn?.ToString()
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
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
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.TrainingProvider_Confirm_Get);
            
            if (long.TryParse(m.Ukprn, out var ukprn) && ukprn != vacancy.TrainingProvider?.Ukprn)
            {
                var provider = await _client.GetTrainingProviderAsync(ukprn);

                return new ConfirmTrainingProviderViewModel
                {
                    EmployerAccountId = m.EmployerAccountId,
                    VacancyId = m.VacancyId,
                    Title = vacancy.Title,
                    Ukprn = provider.Ukprn.Value,
                    ProviderName = provider.Name,
                    ProviderAddress = provider.Address.ToAddressString()
                };
            }

            return new ConfirmTrainingProviderViewModel
            {
                EmployerAccountId = m.EmployerAccountId,
                VacancyId = m.VacancyId,
                Title = vacancy.Title,
                Ukprn = vacancy.TrainingProvider.Ukprn.Value,
                ProviderName = vacancy.TrainingProvider.Name,
                ProviderAddress = vacancy.TrainingProvider.Address.ToAddressString()
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

        public Task<bool> ConfirmProviderExists(long ukprn)
        {
            return _client.GetTrainingProviderExistsAsync(ukprn);
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