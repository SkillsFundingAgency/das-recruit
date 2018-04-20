using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class TrainingProviderOrchestrator : VacancyValidatingOrchestrator<ConfirmTrainingProviderEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingProvider;
        private readonly IEmployerVacancyClient _client;
        private readonly ITrainingProviderService _providerService;

        public TrainingProviderOrchestrator(IEmployerVacancyClient client, ITrainingProviderService providerService, ILogger<TrainingProviderOrchestrator> logger) : base(logger)
        {
            _client = client;
            _providerService = providerService;
        }

        public async Task<SelectTrainingProviderViewModel> GetSelectTrainingProviderViewModel(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            if (!vacancy.CanEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new SelectTrainingProviderViewModel
            {
                Title = vacancy.Title,
                Ukprn = vacancy.TrainingProvider?.Ukprn
            };

            return vm;
        }

        public async Task<SelectTrainingProviderViewModel> GetSelectTrainingProviderViewModel(SelectTrainingProviderEditModel m)
        {
            var vrm = new VacancyRouteModel { EmployerAccountId = m.EmployerAccountId, VacancyId = m.VacancyId };
            var vm = await GetSelectTrainingProviderViewModel(vrm);
            vm.Ukprn = long.TryParse(m.Ukprn, out var submittedUkprn) ? submittedUkprn : default(long);
            return vm;
        }

        public async Task<ConfirmTrainingProviderViewModel> GetConfirmViewModel(SelectTrainingProviderEditModel m)
        {
            var vacancy = await _client.GetVacancyAsync(m.VacancyId);

            CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

            if (!vacancy.CanEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

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
            var vacancyTask = _client.GetVacancyAsync(m.VacancyId);
            var providerTask = _providerService.GetProviderAsync(long.Parse(m.Ukprn));

            Task.WaitAll(new Task[] { vacancyTask, providerTask });

            var vacancy = vacancyTask.Result;
            var provider = providerTask.Result;

            CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

            if (!vacancy.CanEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            vacancy.TrainingProvider = provider;

            return ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateVacancyAsync(vacancy, user)
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