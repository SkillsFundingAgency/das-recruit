using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class LocationOrchestrator
    {
            private readonly IEmployerVacancyClient _employerVacancyClient; 
            private readonly IRecruitVacancyClient _recruitVacancyClient;

        public LocationOrchestrator(
            IEmployerVacancyClient employerVacancyClient, 
            IRecruitVacancyClient recruitVacancyClient)
        {
            _employerVacancyClient = employerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
        }

        public async Task<LocationViewModel> GetLocationViewModelAsync(
            VacancyRouteModel vrm, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_employerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Location_Get);

            return await GetViewModelAsync(vacancy);
        }

        private async Task<LocationViewModel> GetViewModelAsync(Vacancy vacancy)
        {
            var employerData = await _employerVacancyClient.GetEditVacancyInfoAsync(vacancy.EmployerAccountId);
            var legalEntity = employerData.LegalEntities.First();

            var vm = new LocationViewModel();
            vm.PageInfo = Utility.GetPartOnePageInfo(vacancy);

            vm.LegalEntityLocation = legalEntity.Address.ToString();

            if (vacancy.EmployerLocation != null) 
            {
                vm.UseOtherLocation = false;

                if (vacancy.EmployerLocation.ToString() != legalEntity.Address.ToString())
                {
                    vm.UseOtherLocation = true;
                    vm.AddressLine1 = vacancy.EmployerLocation.AddressLine1;
                    vm.AddressLine2 = vacancy.EmployerLocation.AddressLine2;
                    vm.AddressLine3 = vacancy.EmployerLocation.AddressLine3;
                    vm.AddressLine4 = vacancy.EmployerLocation.AddressLine4;
                    vm.Postcode = vacancy.EmployerLocation.Postcode;
                }
            }            
            return vm;
        }
    }
}