using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class ApplicationProcessOrchestrator : EntityValidatingOrchestrator<Vacancy, ApplicationProcessEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ApplicationUrl | VacancyRuleSet.ApplicationInstructions | VacancyRuleSet.EmployerContactDetails;
        private readonly IVacancyClient _client;
        private readonly ILogger<ApplicationProcessOrchestrator> _logger;

        public ApplicationProcessOrchestrator(IVacancyClient client, ILogger<ApplicationProcessOrchestrator> logger) : base(logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<ApplicationProcessViewModel> GetApplicationProcessViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyAsync(vacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new ApplicationProcessViewModel
            {
                Title = vacancy.Title,
                ApplicationInstructions = vacancy.ApplicationInstructions,
                ApplicationUrl = vacancy.ApplicationUrl,
                EmployerContactName = vacancy.EmployerContactName,
                EmployerContactEmail = vacancy.EmployerContactEmail,
                EmployerContactPhone = vacancy.EmployerContactPhone
            };

            return vm;
        }

        public async Task<ApplicationProcessViewModel> GetApplicationProcessViewModelAsync(ApplicationProcessEditModel m)
        {
            var vm = await GetApplicationProcessViewModelAsync(m.VacancyId);

            vm.ApplicationInstructions = m.ApplicationInstructions;
            vm.ApplicationUrl = m.ApplicationUrl;
            vm.EmployerContactName = m.EmployerContactName;
            vm.EmployerContactEmail = m.EmployerContactEmail;
            vm.EmployerContactPhone = m.EmployerContactPhone;

            return vm;
        }

        public async Task<OrchestratorResponse> PostApplicationProcessEditModelAsync(ApplicationProcessEditModel m)
        {
            var vacancy = await _client.GetVacancyAsync(m.VacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            vacancy.ApplicationInstructions = m.ApplicationInstructions;
            vacancy.ApplicationUrl = m.ApplicationUrl;
            vacancy.EmployerContactName = m.EmployerContactName;
            vacancy.EmployerContactEmail = m.EmployerContactEmail;
            vacancy.EmployerContactPhone = m.EmployerContactPhone;

            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateVacancyAsync(vacancy)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ApplicationProcessEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, ApplicationProcessEditModel>();

            mappings.Add(e => e.ApplicationUrl, vm => vm.ApplicationUrl);
            mappings.Add(e => e.ApplicationInstructions, vm => vm.ApplicationInstructions);
            mappings.Add(e => e.EmployerContactName, vm => vm.EmployerContactName);
            mappings.Add(e => e.EmployerContactEmail, vm => vm.EmployerContactEmail);
            mappings.Add(e => e.EmployerContactPhone, vm => vm.EmployerContactPhone);

            return mappings;
        }
    }
}
