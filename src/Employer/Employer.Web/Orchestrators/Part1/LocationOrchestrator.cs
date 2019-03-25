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
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.Extensions;

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
            VacancyRouteModel vrm, EmployerInfoModel employerInfoModel, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_employerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Location_Get);

            return await GetViewModelAsync(vacancy, employerInfoModel);
        }

        public async Task<OrchestratorResponse> PostLocationEditModelAsync(
            LocationEditModel m, EmployerInfoModel employerInfoModel, VacancyUser user)
        {
            if(!m.UseOtherLocation.HasValue)
                return new OrchestratorResponse(false);

            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(
                _employerVacancyClient, _recruitVacancyClient, m, RouteNames.Employer_Post);
            var employerVacancyInfoTask = _employerVacancyClient.GetEditVacancyInfoAsync(m.EmployerAccountId);

            await Task.WhenAll(vacancyTask, employerVacancyInfoTask);
            var vacancy = vacancyTask.Result;
            var editVacancyInfo = employerVacancyInfoTask.Result;

            var legalEntityId = GetLegalEntityId(employerInfoModel, vacancy);

            var selectedOrganisation = 
                editVacancyInfo.LegalEntities.Single(l => l.LegalEntityId == legalEntityId);

            vacancy.LegalEntityName = selectedOrganisation.Name;
            vacancy.LegalEntityId = legalEntityId;

            if(employerInfoModel?.EmployerNameOption != null)
            {
                vacancy.EmployerNameOption = employerInfoModel.EmployerNameOption.Value.ConvertToDomainEntity();
            }

            if (m.UseOtherLocation == true)
            {
                vacancy.EmployerLocation = new Vacancies.Client.Domain.Entities.Address
                {
                    AddressLine1 = m.AddressLine1,
                    AddressLine2 = m.AddressLine2,
                    AddressLine3 = m.AddressLine3,
                    AddressLine4 = m.AddressLine4,
                    Postcode = m.Postcode.AsPostcode()
                };
            }
            else
            {
                vacancy.EmployerLocation = new Vacancies.Client.Domain.Entities.Address
                {
                    AddressLine1 = selectedOrganisation.Address.AddressLine1,
                    AddressLine2 = selectedOrganisation.Address.AddressLine2,
                    AddressLine3 = selectedOrganisation.Address.AddressLine3,
                    AddressLine4 = selectedOrganisation.Address.AddressLine4,
                    Postcode = selectedOrganisation.Address.Postcode.AsPostcode()
                };
            }

            return await ValidateAndExecute(
                vacancy, 
                v => _recruitVacancyClient.Validate(v, ValidationRules),
                v => _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user));
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

        private async Task<LocationViewModel> GetViewModelAsync(Vacancy vacancy, EmployerInfoModel employerInfoModel)
        {
            var employerData = await _employerVacancyClient.GetEditVacancyInfoAsync(vacancy.EmployerAccountId);

            var legalEntityId = GetLegalEntityId(employerInfoModel, vacancy);

            var legalEntity = employerData.LegalEntities.First(l => l.LegalEntityId == legalEntityId);

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

        private long GetLegalEntityId(EmployerInfoModel employerInfoModel, Vacancy vacancy)
        {
            var legalEntityId = employerInfoModel?.LegalEntityId != null ? employerInfoModel?.LegalEntityId : vacancy.LegalEntityId; 

            if (legalEntityId == null || legalEntityId == 0)
            {
                throw new ArgumentNullException("LegalEntityId");
            }
            return legalEntityId.Value;
        }
    }
}