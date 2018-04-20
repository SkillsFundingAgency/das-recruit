﻿using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class ApplicationProcessOrchestrator : VacancyValidatingOrchestrator<ApplicationProcessEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ApplicationUrl | VacancyRuleSet.ApplicationInstructions | VacancyRuleSet.EmployerContactDetails;
        private readonly IEmployerVacancyClient _client;

        public ApplicationProcessOrchestrator(IEmployerVacancyClient client, ILogger<ApplicationProcessOrchestrator> logger) : base(logger)
        {
            _client = client;
        }

        public async Task<ApplicationProcessViewModel> GetApplicationProcessViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            if (!vacancy.CanEdit)
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
            var vrm = new VacancyRouteModel { EmployerAccountId = m.EmployerAccountId, VacancyId = m.VacancyId };
            var vm = await GetApplicationProcessViewModelAsync(vrm);

            vm.ApplicationInstructions = m.ApplicationInstructions;
            vm.ApplicationUrl = m.ApplicationUrl;
            vm.EmployerContactName = m.EmployerContactName;
            vm.EmployerContactEmail = m.EmployerContactEmail;
            vm.EmployerContactPhone = m.EmployerContactPhone;

            return vm;
        }

        public async Task<OrchestratorResponse> PostApplicationProcessEditModelAsync(ApplicationProcessEditModel m, VacancyUser user)
        {
            var vacancy = await _client.GetVacancyAsync(m.VacancyId);

            CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

            if (!vacancy.CanEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            vacancy.ApplicationInstructions = m.ApplicationInstructions;
            vacancy.ApplicationUrl = m.ApplicationUrl;
            vacancy.EmployerContactName = m.EmployerContactName;
            vacancy.EmployerContactEmail = m.EmployerContactEmail;
            vacancy.EmployerContactPhone = m.EmployerContactPhone;

            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateVacancyAsync(vacancy, user)
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
