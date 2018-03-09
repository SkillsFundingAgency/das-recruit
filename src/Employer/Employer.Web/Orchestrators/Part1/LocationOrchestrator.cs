using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels.Location;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class LocationOrchestrator
    {
        private readonly IVacancyClient _client;

        public LocationOrchestrator(IVacancyClient client)
        {
            _client = client;
        }

        public async Task<LocationViewModel> GetLocationViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);
            
            var vm = new LocationViewModel
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

        public async Task<LocationViewModel> GetLocationViewModelAsync(LocationEditModel m)
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

        public async Task PostLocationEditModelAsync(LocationEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            vacancy.OrganisationId = m.SelectedOrganisationId?.Trim();
            vacancy.Location = new Address
            {
                AddressLine1 = m.AddressLine1?.Trim(),
                AddressLine2 = m.AddressLine2?.Trim(),
                AddressLine3 = m.AddressLine3?.Trim(),
                AddressLine4 = m.AddressLine4?.Trim(),
                Postcode = m.Postcode?.AsPostCode()
            };
            
            await _client.UpdateVacancyAsync(vacancy);
        }
    }
}
