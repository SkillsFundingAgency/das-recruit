using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class EmployerOrchestrator
    {
        private readonly IVacancyClient _client;

        public EmployerOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public async Task<EmployerViewModel> GetLocationViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);
            
            var vm = new EmployerViewModel
            {
                Organisations = new List<LocationOrganisationViewModel>
                {
                    new LocationOrganisationViewModel{Id = "A", Name = "Organisation A"},
                    new LocationOrganisationViewModel{Id = "B", Name = "Organisation B"},
                    new LocationOrganisationViewModel{Id = "C", Name = "Organisation C"}
                },
                SelectedOrganisationId = vacancy.OrganisationId,
            };

            if (vacancy.Location != null)
            {
                vm.AddressLine1 = vacancy.Location.AddressLine1;
                vm.AddressLine2 = vacancy.Location.AddressLine2;
                vm.AddressLine3 = vacancy.Location.AddressLine3;
                vm.AddressLine4 = vacancy.Location.AddressLine4;
                vm.Postcode = vacancy.Location.Postcode;
            }

            return vm;
        }

        public async Task<EmployerViewModel> GetLocationViewModelAsync(EmployerEditModel m)
        {
            var vm = await GetLocationViewModelAsync(m.VacancyId);

            vm.SelectedOrganisationId = m.SelectedOrganisationId;
            vm.AddressLine1 = m.AddressLine1;
            vm.AddressLine2 = m.AddressLine2;
            vm.AddressLine3 = m.AddressLine3;
            vm.AddressLine4 = m.AddressLine4;
            vm.Postcode = m.Postcode;

            return vm;
        }

        public async Task PostLocationEditModelAsync(EmployerEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (vacancy.Status != VacancyStatus.Draft)
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            
            vacancy.OrganisationId = m.SelectedOrganisationId?.Trim();
            vacancy.Location = new Address
            {
                AddressLine1 = m.AddressLine1,
                AddressLine2 = m.AddressLine2,
                AddressLine3 = m.AddressLine3,
                AddressLine4 = m.AddressLine4,
                Postcode = m.Postcode?.AsPostcode()
            };
            
            await _client.UpdateVacancyAsync(vacancy, false);
        }
    }
}
