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
using Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Employer.Web.Mappings;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class LocationOrchestrator : EntityValidatingOrchestrator<Vacancy, LocationEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerAddress;
        private readonly IEmployerVacancyClient _employerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public LocationOrchestrator(
            IEmployerVacancyClient employerVacancyClient,
            IRecruitVacancyClient recruitVacancyClient,
            ILogger<LocationOrchestrator> logger,
            IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _employerVacancyClient = employerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
            _reviewSummaryService = reviewSummaryService;
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

        public async Task<LocationViewModel> GetLocationViewModelAsync(
            VacancyRouteModel vrm, VacancyEmployerInfoModel employerInfoModel, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_employerVacancyClient, _recruitVacancyClient,
                vrm, RouteNames.Location_Get);

            var legalEntityId = employerInfoModel?.LegalEntityId != null ? employerInfoModel?.LegalEntityId : vacancy.LegalEntityId;

            var vm = new LocationViewModel();
            vm.PageInfo = Utility.GetPartOnePageInfo(vacancy);

            var employerProfile =
                await _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, legalEntityId.GetValueOrDefault());

            var allLocations = await GetAllAvailableLocationsAsync(employerProfile);
            
            vm.AvailableLocations = allLocations.Select(a => a.ToAddressString()).ToList();

            var hasLegalEntityChanged = employerInfoModel?.HasLegalEntityChanged ?? false;

            if (vacancy.EmployerLocation != null && hasLegalEntityChanged == false)
            {
                vm.SelectedLocation = 
                    GetMatchingAddress(vacancy.EmployerLocation.ToAddressString(), allLocations)
                        .ToAddressString();
            } 

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetLocationFieldIndicators());
            }
            return vm;
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

            var legalEntityId = employerInfoModel?.LegalEntityId == null ? vacancy.LegalEntityId : employerInfoModel.LegalEntityId;
            var selectedOrganisation =
                    editVacancyInfo.LegalEntities.Single(l => l.LegalEntityId == legalEntityId);

            var employerProfile =
                await _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, selectedOrganisation.LegalEntityId);

            var allLocations = await GetAllAvailableLocationsAsync(employerProfile);

            var newLocation = 
                locationEditModel.SelectedLocation == LocationViewModel.UseOtherLocationConst 
                ? locationEditModel.ToAddressString() 
                : locationEditModel.SelectedLocation;

            var matchingAddress = GetMatchingAddress(newLocation, allLocations);

            vacancy.EmployerLocation = matchingAddress != null ? matchingAddress : ConvertToDomainAddress(locationEditModel);

            //if cookie is found then update legal entity and name option from cookie
            if (employerInfoModel != null)
            {
                vacancy.LegalEntityName = selectedOrganisation.Name;
                vacancy.LegalEntityId = employerInfoModel.LegalEntityId.GetValueOrDefault();
                vacancy.EmployerNameOption = employerInfoModel.EmployerNameOption?.ConvertToDomainOption();
            }

            return await ValidateAndExecute(
                vacancy,
                v => _recruitVacancyClient.Validate(v, ValidationRules),
                async v =>
                {
                    await _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                    await UpdateEmployerProfile(employerInfoModel, employerProfile, matchingAddress == null ? vacancy.EmployerLocation : null, user);
                });
        }

        private Address ConvertToDomainAddress(LocationEditModel locationEditModel)
        {
            return new Address 
                {
                    AddressLine1 = locationEditModel.AddressLine1,
                    AddressLine2 = locationEditModel.AddressLine2,
                    AddressLine3 = locationEditModel.AddressLine3,
                    AddressLine4 = locationEditModel.AddressLine4,
                    Postcode = locationEditModel.Postcode.AsPostcode()
                };
        }

        private async Task UpdateEmployerProfile(VacancyEmployerInfoModel employerInfoModel, 
            EmployerProfile employerProfile, Address address, VacancyUser user)
        {
            var updateProfile = false;
            if (employerInfoModel != null && employerInfoModel.EmployerNameOption == EmployerNameOptionViewModel.NewTradingName)
            {
                updateProfile = true;
                employerProfile.TradingName = employerInfoModel.NewTradingName;
            }
            if (address != null)
            {
                updateProfile = true;
                employerProfile.OtherLocations.Add(address);
            }
            if (updateProfile)    
            {
                await _recruitVacancyClient.UpdateEmployerProfileAsync(employerProfile, user);
            }
        }

        private Address GetMatchingAddress(string locationToMatch, IEnumerable<Address> allLocations)
        {
            var matchingLocation = 
                    allLocations
                        .FirstOrDefault(l => l.ToAddressString().Equals(locationToMatch, StringComparison.OrdinalIgnoreCase));
            return matchingLocation;
        }

        private async Task<List<Address>> GetAllAvailableLocationsAsync(EmployerProfile employerProfile)
        {
            var employerData = await _employerVacancyClient.GetEditVacancyInfoAsync(employerProfile.EmployerAccountId);
            var legalEntity = employerData.LegalEntities.First(l => l.LegalEntityId == employerProfile.LegalEntityId);
            
            var locations = new List<Address>();
            locations.Add(legalEntity.Address.ConvertToDomainAddress());
            locations.AddRange(employerProfile.OtherLocations);
            return locations;
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
    }
}