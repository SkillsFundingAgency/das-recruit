using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class EmployerContactDetailsOrchestrator : EntityValidatingOrchestrator<Vacancy, EmployerContactDetailsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerContactDetails;
        private readonly IEmployerVacancyClient _client;

        public EmployerContactDetailsOrchestrator(IEmployerVacancyClient client, ILogger<EmployerContactDetailsOrchestrator> logger) : base(logger)
        {
            _client = client;
        }

        public async Task<EmployerContactDetailsViewModel> GetEmployerContactDetailsViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            if (!vacancy.CanEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new EmployerContactDetailsViewModel
            {
                Title = vacancy.Title,
                EmployerContactName = vacancy.EmployerContactName,
                EmployerContactEmail = vacancy.EmployerContactEmail,
                EmployerContactPhone = vacancy.EmployerContactPhone
            };

            return vm;
        }

        public async Task<EmployerContactDetailsViewModel> GetEmployerContactDetailsViewModelAsync(EmployerContactDetailsEditModel m)
        {
            var vm = await GetEmployerContactDetailsViewModelAsync((VacancyRouteModel)m);

            vm.EmployerContactName = m.EmployerContactName;
            vm.EmployerContactEmail = m.EmployerContactEmail;
            vm.EmployerContactPhone = m.EmployerContactPhone;

            return vm;
        }

        public async Task<OrchestratorResponse> PostEmployerContactDetailsEditModelAsync(EmployerContactDetailsEditModel m, VacancyUser user)
        {
            var vacancy = await _client.GetVacancyAsync(m.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

            if (!vacancy.CanEdit)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            vacancy.EmployerContactName = m.EmployerContactName;
            vacancy.EmployerContactEmail = m.EmployerContactEmail;
            vacancy.EmployerContactPhone = m.EmployerContactPhone;

            return await ValidateAndExecute(
                vacancy,
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, EmployerContactDetailsEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, EmployerContactDetailsEditModel>();

            mappings.Add(e => e.EmployerContactName, vm => vm.EmployerContactName);
            mappings.Add(e => e.EmployerContactEmail, vm => vm.EmployerContactEmail);
            mappings.Add(e => e.EmployerContactPhone, vm => vm.EmployerContactPhone);

            return mappings;
        }
    }
}
