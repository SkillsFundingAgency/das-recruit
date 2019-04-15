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
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
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

        public async Task<LocationViewModel> GetLocationViewModelAsync(
            VacancyRouteModel vrm, VacancyEmployerInfoModel employerInfoModel, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Location_Get);

            var vm = new LocationViewModel();
            vm.PageInfo = Utility.GetPartOnePageInfo(vacancy);
           
            long? selectedOrganisationId = employerInfoModel?.LegalEntityId;
            
            var hasLegalEntityChanged = employerInfoModel?.HasLegalEntityChanged ?? false;

            var legalEntityId = selectedOrganisationId.HasValue ? selectedOrganisationId : vacancy.LegalEntityId;

            var employerProfile =
                await _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, legalEntityId.GetValueOrDefault());

            var allLocations = await GetAllAvailableLocationsAsync(employerProfile, vacancy, vrm.Ukprn);

            vm.AvailableLocations = allLocations.Select(a => a.ToAddressString()).ToList();

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

        private async Task<List<Address>> GetAllAvailableLocationsAsync(EmployerProfile employerProfile, Vacancy vacancy, long ukprn)
        {
            var employerData = await _providerVacancyClient.GetProviderEditVacancyInfoAsync(ukprn);
            var employerInfo = employerData.Employers.Single(e => e.EmployerAccountId == vacancy.EmployerAccountId);
            var legalEntity = employerInfo.LegalEntities.First(l => l.LegalEntityId == employerProfile.LegalEntityId);
            var locations = new List<Address>();
            locations.Add(legalEntity.Address.ConvertToDomainAddress());
            locations.AddRange(employerProfile.OtherLocations);
            return locations;
        }

        private Address GetMatchingAddress(string locationToMatch, IEnumerable<Address> allLocations)
        {
            var matchingLocation =
                allLocations
                    .FirstOrDefault(l => l.ToAddressString().Equals(locationToMatch, StringComparison.OrdinalIgnoreCase));
            return matchingLocation;
        }

        public async Task<VacancyEmployerInfoModel> GetVacancyEmployerInfoModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Location_Get);

            var model = new VacancyEmployerInfoModel()
            {
                VacancyId = vacancy.Id,
                LegalEntityId = vacancy.LegalEntityId == 0 ? (long?)null : vacancy.LegalEntityId
            };
            if (vacancy.EmployerNameOption.HasValue)
                model.EmployerNameOption = vacancy.EmployerNameOption.Value.ConvertToModelOption();
            return model;
        }

        public async Task<OrchestratorResponse> PostLocationEditModelAsync(
            LocationEditModel locationEditModel, VacancyUser user, long ukprn,
            VacancyEmployerInfoModel employerInfoModel)
        {
            if (string.IsNullOrEmpty(locationEditModel.SelectedLocation))
                return new OrchestratorResponse(false);

            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_providerVacancyClient,
                _recruitVacancyClient, locationEditModel, RouteNames.Location_Post);

            var employerVacancyInfo =
                await _providerVacancyClient.GetProviderEmployerVacancyDataAsync(ukprn, vacancy.EmployerAccountId);

            LegalEntity selectedOrganisation;
            //if cookie is found update legal entity and name option
            if (employerInfoModel != null)
            {
                selectedOrganisation =
                    employerVacancyInfo.LegalEntities.Single(l => l.LegalEntityId == employerInfoModel.LegalEntityId);
                vacancy.LegalEntityName = selectedOrganisation.Name;
                vacancy.LegalEntityId = employerInfoModel.LegalEntityId.GetValueOrDefault();
                vacancy.EmployerNameOption = employerInfoModel.EmployerNameOption?.ConvertToDomainOption();
            }
            else
            {
                selectedOrganisation =
                    employerVacancyInfo.LegalEntities.Single(l => l.LegalEntityId == vacancy.LegalEntityId);                
            }

            long? selectedOrganisationId = selectedOrganisation.LegalEntityId;

            var legalEntityId = selectedOrganisationId.HasValue ? selectedOrganisationId : vacancy.LegalEntityId;

            var employerProfile =
                await _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, legalEntityId.GetValueOrDefault());

            var allLocations = await GetAllAvailableLocationsAsync(employerProfile, vacancy, ukprn);

            var newLocation =
                locationEditModel.SelectedLocation == LocationViewModel.UseOtherLocationConst
                    ? locationEditModel.ToAddressString()
                    : locationEditModel.SelectedLocation;

            var matchingAddress = GetMatchingAddress(newLocation, allLocations);

            vacancy.EmployerLocation = matchingAddress != null ? matchingAddress : ConvertToDomainAddress(locationEditModel);
                 
            return await ValidateAndExecute(
                vacancy,
                v => _recruitVacancyClient.Validate(v, ValidationRules),
                async v =>
                {
                    await _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                    await UpdateEmployerProfileAsync(employerInfoModel, employerProfile, matchingAddress == null ? vacancy.EmployerLocation : null, user);
                });            
        }

        private async Task UpdateEmployerProfileAsync(VacancyEmployerInfoModel employerInfoModel,
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
    }
}