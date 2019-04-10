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
using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.EmployerName;
using Address = Esfa.Recruit.Vacancies.Client.Domain.Entities.Address;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class LocationOrchestrator : EntityValidatingOrchestrator<Vacancy, LocationEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerAddress;
        private const string UseOtherLocation = "UseOtherLocation";
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
            VacancyRouteModel vrm, VacancyEmployerInfoModel employerInfoModel, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_employerVacancyClient, _recruitVacancyClient,
                vrm, RouteNames.Location_Get);
            return await GetViewModelAsync(vacancy,
                employerInfoModel?.LegalEntityId, employerInfoModel?.HasLegalEntityChanged ?? false);
        }

        public async Task<OrchestratorResponse> PostLocationEditModelAsync(
            LocationEditModel locationEditModel, VacancyEmployerInfoModel employerInfoModel, VacancyUser user)
        {            
            if (string.IsNullOrEmpty(locationEditModel.SelectedLocation))
                return new OrchestratorResponse(false);

            var vacancyTask = Utility.GetAuthorisedVacancyForEditAsync(
                _employerVacancyClient, _recruitVacancyClient, locationEditModel, RouteNames.Employer_Post);
            var employerVacancyInfoTask =
                _employerVacancyClient.GetEditVacancyInfoAsync(locationEditModel.EmployerAccountId);
            await Task.WhenAll(vacancyTask, employerVacancyInfoTask);
            var vacancy = vacancyTask.Result;

            var editVacancyInfo = employerVacancyInfoTask.Result;

            LegalEntity selectedOrganisation;
            //if cookie is found update legal entity and name option
            if (employerInfoModel != null)
            {
                selectedOrganisation =
                    editVacancyInfo.LegalEntities.Single(l => l.LegalEntityId == employerInfoModel.LegalEntityId);

                vacancy.LegalEntityName = selectedOrganisation.Name;
                vacancy.LegalEntityId = employerInfoModel.LegalEntityId.GetValueOrDefault();

                vacancy.EmployerNameOption = employerInfoModel.EmployerNameOption?.ConvertToDomainOption();
            }
            else
            {
                selectedOrganisation =
                    editVacancyInfo.LegalEntities.Single(l => l.LegalEntityId == vacancy.LegalEntityId);
            }

            var employerProfile =
                await _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, selectedOrganisation.LegalEntityId);

            var newLocation = 
                locationEditModel.SelectedLocation == LocationViewModel.UseOtherLocationConst 
                ? locationEditModel.OtherLocationString 
                : locationEditModel.SelectedLocation;

            var matchingAddress = GetMatchingAddress(newLocation, employerProfile, selectedOrganisation);

            if (matchingAddress != null)
            {
                vacancy.EmployerLocation = matchingAddress;
            }
            else
            {
                vacancy.EmployerLocation = new Address 
                {
                    AddressLine1 = locationEditModel.AddressLine1,
                    AddressLine2 = locationEditModel.AddressLine2,
                    AddressLine3 = locationEditModel.AddressLine3,
                    AddressLine4 = locationEditModel.AddressLine4,
                    Postcode = locationEditModel.Postcode.AsPostcode()
                };
            }
            
            return await ValidateAndExecute(
                vacancy,
                v => _recruitVacancyClient.Validate(v, ValidationRules),
                async v =>
                {
                    await _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);

                    var updateProfile = false;
                    if (employerInfoModel != null && employerInfoModel.EmployerNameOption == EmployerNameOptionViewModel.NewTradingName)
                    {
                        updateProfile = true;
                        employerProfile.TradingName = employerInfoModel.NewTradingName;
                    }
                    if (matchingAddress == null)
                    {
                        updateProfile = true;
                        employerProfile.OtherLocations.Add(vacancy.EmployerLocation);
                    }
                    if (updateProfile)    
                    {
                        await _recruitVacancyClient.UpdateEmployerProfileAsync(employerProfile, user);
                    }
                });
        }

        private Address GetMatchingAddress(string currentLocation, EmployerProfile employerProfile, LegalEntity legalEntity)
        {
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            var registeredLocation = legalEntity.Address.ToString();
            if (comparer.Compare(currentLocation, registeredLocation) == 0)
            {
                return legalEntity.Address.ConvertToDomainAddress();
            }
            var otherLocations = employerProfile.OtherLocations;
            if (otherLocations?.Any() == true)
            {
                foreach (var location in otherLocations)
                {
                    if (comparer.Compare(currentLocation, location.ToString()) == 0)
                    {
                        return location;
                    }
                }
            }
            return null;
        }

        public async Task<VacancyEmployerInfoModel> GetVacancyEmployerInfoModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _employerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Employer_Post);

            var model = new VacancyEmployerInfoModel() {
                VacancyId = vacancy.Id,
                LegalEntityId = vacancy.LegalEntityId == 0 ? (long?) null : vacancy.LegalEntityId
            };
            if (vacancy.EmployerNameOption.HasValue)
                model.EmployerNameOption = vacancy.EmployerNameOption.Value.ConvertToModelOption();
            return model;
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

        private async Task<LocationViewModel> GetViewModelAsync(Vacancy vacancy,
            long? selectedOrganisationId, bool hasLegalEntityChanged)
        {
            var employerData = await _employerVacancyClient.GetEditVacancyInfoAsync(vacancy.EmployerAccountId);
            
            var legalEntityId = selectedOrganisationId.HasValue ? selectedOrganisationId : vacancy.LegalEntityId;

            var legalEntity = employerData.LegalEntities.First(l => l.LegalEntityId == legalEntityId);
            if (legalEntity == null)
                throw new ArgumentNullException("Legal entity is required for location");

            var employerProfile =
                await _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, legalEntityId.GetValueOrDefault());

            var vm = new LocationViewModel();
            vm.PageInfo = Utility.GetPartOnePageInfo(vacancy);

            vm.AvailableLocations.Add(legalEntity.Address.ToString());
            
            vm.AvailableLocations.AddRange(employerProfile.OtherLocations.Select(x=>x.ToString()));

            if (vacancy.EmployerLocation != null && hasLegalEntityChanged == false)
            {
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;
                var employerLocation = vacancy.EmployerLocation.ToString();
                var otherLocations = vm.AvailableLocations;
                foreach (var location in otherLocations)
                {
                    if (comparer.Compare(employerLocation, location) == 0)
                    {
                        vm.SelectedLocation = location;
                    }
                }
            }        
            return vm;
        }
    }
}