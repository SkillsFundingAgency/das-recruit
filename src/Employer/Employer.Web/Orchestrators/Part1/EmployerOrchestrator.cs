using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class EmployerOrchestrator : EntityValidatingOrchestrator<Vacancy, EmployerEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerName | VacancyRuleSet.EmployerAddress;
        private readonly IEmployerVacancyClient _client;

        public EmployerOrchestrator(IEmployerVacancyClient client, ILogger<EmployerOrchestrator> logger) : base(logger)
        {
            _client = client;
        }

        public async Task<EmployerViewModel> GetEmployerViewModelAsync(VacancyRouteModel vrm)
        {
            var getEmployerDataTask = _client.GetEditVacancyInfo(vrm.EmployerAccountId);
            var getVacancyTask = _client.GetVacancyAsync(vrm.VacancyId);

            await Task.WhenAll(getEmployerDataTask, getVacancyTask);

            var employerData = getEmployerDataTask.Result;
            var vacancy = getVacancyTask.Result;

            if (!vacancy.CanEdit)
            {
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            var vm = new EmployerViewModel
            {
                Organisations = employerData.LegalEntities.Select(MapLegalEntitiesToOrgs).ToList(),
                SelectedOrganisationName = vacancy.EmployerName
            };

            if (vacancy.EmployerLocation != null)
            {
                vm.AddressLine1 = vacancy.EmployerLocation.AddressLine1;
                vm.AddressLine2 = vacancy.EmployerLocation.AddressLine2;
                vm.AddressLine3 = vacancy.EmployerLocation.AddressLine3;
                vm.AddressLine4 = vacancy.EmployerLocation.AddressLine4;
                vm.Postcode = vacancy.EmployerLocation.Postcode;
            }

            return vm;
        }

        public async Task<EmployerViewModel> GetEmployerViewModelAsync(EmployerEditModel m)
        {
            var vrm = new VacancyRouteModel { EmployerAccountId = m.EmployerAccountId, VacancyId = m.VacancyId };
            var vm = await GetEmployerViewModelAsync(vrm);

            vm.SelectedOrganisationName = m.SelectedOrganisationName;
            vm.AddressLine1 = m.AddressLine1;
            vm.AddressLine2 = m.AddressLine2;
            vm.AddressLine3 = m.AddressLine3;
            vm.AddressLine4 = m.AddressLine4;
            vm.Postcode = m.Postcode;

            return vm;
        }

        public async Task<OrchestratorResponse> PostEmployerEditModelAsync(EmployerEditModel m)
        {
            var vacancy = await _client.GetVacancyAsync(m.VacancyId);

            if (!vacancy.CanEdit)
            {
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            vacancy.EmployerName = m.SelectedOrganisationName?.Trim();
            vacancy.EmployerLocation = new Vacancies.Client.Domain.Entities.Address
            {
                AddressLine1 = m.AddressLine1,
                AddressLine2 = m.AddressLine2,
                AddressLine3 = m.AddressLine3,
                AddressLine4 = m.AddressLine4,
                Postcode = m.Postcode.AsPostcode()
            };

            return await ValidateAndExecute(
                vacancy, 
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateVacancyAsync(vacancy)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, EmployerEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, EmployerEditModel>();

            mappings.Add(e => e.EmployerName, vm => vm.SelectedOrganisationName);
            mappings.Add(e => e.EmployerLocation.AddressLine1, vm => vm.AddressLine1);
            mappings.Add(e => e.EmployerLocation.AddressLine2, vm => vm.AddressLine2);
            mappings.Add(e => e.EmployerLocation.AddressLine3, vm => vm.AddressLine3);
            mappings.Add(e => e.EmployerLocation.AddressLine4, vm => vm.AddressLine4);
            mappings.Add(e => e.EmployerLocation.Postcode, vm => vm.Postcode);

            return mappings;
        }

        private LocationOrganisationViewModel MapLegalEntitiesToOrgs(LegalEntity data)
        {
            return new LocationOrganisationViewModel { Id = data.LegalEntityId.ToString(), Name = data.Name };
        }
    }
}
