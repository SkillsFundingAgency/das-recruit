using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class AboutEmployerOrchestrator : EntityValidatingOrchestrator<Vacancy, AboutEmployerEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerDescription | VacancyRuleSet.EmployerWebsiteUrl;
        private readonly IVacancyClient _client;
        private readonly ILogger<AboutEmployerOrchestrator> _logger;

        public AboutEmployerOrchestrator(IVacancyClient client, ILogger<AboutEmployerOrchestrator> logger) : base(logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<AboutEmployerViewModel> GetAboutEmployerViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new AboutEmployerViewModel
            {
                Title = vacancy.Title,
                EmployerDescription = vacancy.EmployerDescription,
                EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl
            };

            return vm;
        }

        public async Task<AboutEmployerViewModel> GetAboutEmployerViewModelAsync(AboutEmployerEditModel m)
        {
            var vm = await GetAboutEmployerViewModelAsync(m.VacancyId);

            vm.EmployerDescription = m.EmployerDescription;
            vm.EmployerWebsiteUrl = m.EmployerWebsiteUrl;

            return vm;
        }

        public async Task<OrchestratorResponse> PostAboutEmployerEditModelAsync(AboutEmployerEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            vacancy.EmployerDescription = m.EmployerDescription;
            vacancy.EmployerWebsiteUrl = m.EmployerWebsiteUrl;

            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateVacancyAsync(vacancy)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, AboutEmployerEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, AboutEmployerEditModel>();

            mappings.Add(e => e.EmployerDescription, vm => vm.EmployerDescription);
            mappings.Add(e => e.EmployerWebsiteUrl, vm => vm.EmployerWebsiteUrl);

            return mappings;
        }
    }
}
