using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using System;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class LocationOrchestrator : EntityValidatingOrchestrator<Vacancy, LocationEditModel>
    {
        private const VacancyRuleSet ValidationRules =  VacancyRuleSet.EmployerAddress;
        private readonly IEmployerVacancyClient _employerVacancyClient; 
        private readonly IRecruitVacancyClient _recruitVacancyClient;

        public LocationOrchestrator(
            IEmployerVacancyClient employerVacancyClient, 
            IRecruitVacancyClient recruitVacancyClient,
            ILogger<LocationOrchestrator> logger) : base(logger)
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

        public async Task<OrchestratorResponse> PostLocationEditModelAsync(LocationEditModel m, VacancyUser user)
        {
            if(!m.UseOtherLocation.HasValue)
                return new OrchestratorResponse(false);

            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_employerVacancyClient, _recruitVacancyClient, m, RouteNames.Employer_Post);
            var employerVacancyInfoTask = _employerVacancyClient.GetEditVacancyInfoAsync(m.EmployerAccountId);

            await Task.WhenAll(vacancyTask, employerVacancyInfoTask);
            var vacancy = vacancyTask.Result;
            var employerVacancyInfo = employerVacancyInfoTask.Result;

            if (m.UseOtherLocation == true)
            {
                vacancy.EmployerLocation = new Vacancies.Client.Domain.Entities.Address
                {
                    AddressLine1 = m.AddressLine1,
                    AddressLine2 = m.AddressLine2,
                    AddressLine3 = m.AddressLine3,
                    AddressLine4 = m.AddressLine4,
                    Postcode = m.Postcode.AsPostcode(),
                    Latitude = null,
                    Longitude = null
                };
            }
            else
            {
                var selectedOrganisation = employerVacancyInfo.LegalEntities.SingleOrDefault(x => x.LegalEntityId == vacancy.LegalEntityId);

                vacancy.EmployerLocation = new Vacancies.Client.Domain.Entities.Address
                {
                    AddressLine1 = selectedOrganisation.Address.AddressLine1,
                    AddressLine2 = selectedOrganisation.Address.AddressLine2,
                    AddressLine3 = selectedOrganisation.Address.AddressLine3,
                    AddressLine4 = selectedOrganisation.Address.AddressLine4,
                    Postcode = selectedOrganisation.Address.Postcode.AsPostcode(),
                    Latitude = null,
                    Longitude = null
                };
            }

            return await ValidateAndExecute(
                vacancy, 
                v => _recruitVacancyClient.Validate(v, ValidationRules),
                async v => _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user));
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, LocationEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, LocationEditModel>();

            mappings.Add(e => e.EmployerLocation.AddressLine1, vm => vm.AddressLine1);
            mappings.Add(e => e.EmployerLocation.AddressLine2, vm => vm.AddressLine2);
            mappings.Add(e => e.EmployerLocation.AddressLine3, vm => vm.AddressLine3);
            mappings.Add(e => e.EmployerLocation.AddressLine4, vm => vm.AddressLine4);
            mappings.Add(e => e.EmployerLocation.Postcode, vm => vm.Postcode);

            return mappings;
        }

        private async Task<LocationViewModel> GetViewModelAsync(Vacancy vacancy)
        {
            var employerData = await _employerVacancyClient.GetEditVacancyInfoAsync(vacancy.EmployerAccountId);
            var legalEntity = employerData.LegalEntities.First(l => l.LegalEntityId == vacancy.LegalEntityId);

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