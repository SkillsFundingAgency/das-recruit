using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Provider.Web.Model;
using Esfa.Recruit.Provider.Web.Extensions;
using System;
using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.EmployerName;
using Address = Esfa.Recruit.Vacancies.Client.Domain.Entities.Address;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class LocationOrchestrator : EntityValidatingOrchestrator<Vacancy, LocationEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerAddress;
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public LocationOrchestrator(IProviderVacancyClient providerVacancyClient, 
            IRecruitVacancyClient recruitVacancyClient, ILogger<LocationOrchestrator> logger, IReviewSummaryService reviewSummaryService)
            : base(logger)
        {
            _providerVacancyClient = providerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<VacancyEmployerInfoModel> GetVacancyEmployerInfoModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Location_Get);

            var model = new VacancyEmployerInfoModel() {
                VacancyId = vacancy.Id,
                LegalEntityId = vacancy.LegalEntityId == 0 ? (long?)null : vacancy.LegalEntityId
            };
            if (vacancy.EmployerNameOption.HasValue)
                model.EmployerIdentityOption = vacancy.EmployerNameOption.Value.ConvertToModelOption();
            return model;
        }

        public async Task<LocationViewModel> GetLocationViewModelAsync(
            VacancyRouteModel vrm, VacancyEmployerInfoModel employerInfoModel, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Location_Get);

            var legalEntityId = employerInfoModel?.LegalEntityId ?? vacancy.LegalEntityId;

            var vm = new LocationViewModel();
            vm.PageInfo = Utility.GetPartOnePageInfo(vacancy);

            var employerProfile =
                await _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, legalEntityId);

            var allLocations = await GetAllAvailableLocationsAsync(employerProfile, vacancy, vrm.Ukprn);

            vm.AvailableLocations = allLocations.Select(a => a.ToAddressString()).ToList();

            var hasLegalEntityChanged = employerInfoModel?.HasLegalEntityChanged ?? false;

            if (vacancy.EmployerLocation != null && hasLegalEntityChanged == false)
            {
                var matchingAddress = GetMatchingAddress(vacancy.EmployerLocation.ToAddressString(), allLocations);
                if (matchingAddress == null)
                {
                    vm.SelectedLocation = LocationViewModel.UseOtherLocationConst;
                    vm.SetLocation(vacancy.EmployerLocation);
                }
                else
                {
                    vm.SelectedLocation = matchingAddress.ToAddressString();
                }
            }

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetLocationFieldIndicators());
            }
            return vm;
        }

        public async Task<OrchestratorResponse> PostLocationEditModelAsync(
            LocationEditModel locationEditModel, VacancyUser user, long ukprn,
            VacancyEmployerInfoModel employerInfoModel)
        {
            if (string.IsNullOrEmpty(locationEditModel.SelectedLocation))
                return new OrchestratorResponse(false);

            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_providerVacancyClient,
                _recruitVacancyClient, locationEditModel, RouteNames.Location_Post);
            var legalEntityId = employerInfoModel?.LegalEntityId ?? vacancy.LegalEntityId;

            var employerVacancyInfoTask = _providerVacancyClient.GetProviderEmployerVacancyDataAsync(ukprn, vacancy.EmployerAccountId);
            var employerProfileTask = _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, legalEntityId);
            await Task.WhenAll(employerProfileTask, employerVacancyInfoTask);

            var employerVacancyInfo = employerVacancyInfoTask.Result;
            var employerProfile = employerProfileTask.Result;
            var selectedOrganisation = employerVacancyInfo.LegalEntities.Single(l => l.LegalEntityId == legalEntityId);
            var allLocations = await GetAllAvailableLocationsAsync(employerProfile, vacancy, ukprn);
            var newLocation =
                locationEditModel.SelectedLocation == LocationViewModel.UseOtherLocationConst
                    ? locationEditModel.ToAddressString()
                    : locationEditModel.SelectedLocation;

            var matchingAddress = GetMatchingAddress(newLocation, allLocations);

            vacancy.EmployerLocation = matchingAddress ?? ConvertToDomainAddress(locationEditModel);

            //if cookie is found update legal entity and name option
            if (employerInfoModel != null)
            {
                vacancy.LegalEntityName = selectedOrganisation.Name;
                vacancy.LegalEntityId = employerInfoModel.LegalEntityId.GetValueOrDefault();
                vacancy.EmployerNameOption = employerInfoModel.EmployerIdentityOption?.ConvertToDomainOption();
            }

            return await ValidateAndExecute(
                vacancy,
                v => _recruitVacancyClient.Validate(v, ValidationRules),
                async v =>
                {
                    await _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                    await UpdateEmployerProfileAsync(employerInfoModel, employerProfile, matchingAddress == null ? vacancy.EmployerLocation : null, user);
                });            
        }

        private Address GetMatchingAddress(string locationToMatch, IEnumerable<Address> allLocations)
        {
            var matchingLocation =
                allLocations
                    .FirstOrDefault(l => l.ToAddressString().Equals(locationToMatch, StringComparison.OrdinalIgnoreCase));
            return matchingLocation;
        }

        private Address ConvertToDomainAddress(LocationEditModel locationEditModel)
        {
            return new Address {
                AddressLine1 = locationEditModel.AddressLine1,
                AddressLine2 = locationEditModel.AddressLine2,
                AddressLine3 = locationEditModel.AddressLine3,
                AddressLine4 = locationEditModel.AddressLine4,
                Postcode = locationEditModel.Postcode.AsPostcode()
            };
        }

        private async Task UpdateEmployerProfileAsync(VacancyEmployerInfoModel employerInfoModel,
            EmployerProfile employerProfile, Address address, VacancyUser user)
        {
            var updateProfile = false;
            if (employerInfoModel != null && employerInfoModel.EmployerIdentityOption == EmployerIdentityOption.NewTradingName)
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

        private async Task<List<Address>> GetAllAvailableLocationsAsync(EmployerProfile employerProfile, Vacancy vacancy, long ukprn)
        {
            var providerData = await _providerVacancyClient.GetProviderEditVacancyInfoAsync(ukprn);
            var employerInfo = providerData.Employers.Single(e => e.EmployerAccountId == vacancy.EmployerAccountId);
            var legalEntity = employerInfo.LegalEntities.First(l => l.LegalEntityId == employerProfile.LegalEntityId);
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