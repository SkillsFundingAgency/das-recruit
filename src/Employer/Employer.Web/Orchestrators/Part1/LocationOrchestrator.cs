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
using Address = Esfa.Recruit.Vacancies.Client.Domain.Entities.Address;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class LocationOrchestrator : VacancyValidatingOrchestrator<LocationEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerAddress;
        private readonly IEmployerVacancyClient _employerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IGetAddressesClient _getAddressesClient;
        private readonly IUtility _utility;

        public LocationOrchestrator(
            IEmployerVacancyClient employerVacancyClient,
            IRecruitVacancyClient recruitVacancyClient,
            ILogger<LocationOrchestrator> logger,
            IReviewSummaryService reviewSummaryService,
            IGetAddressesClient getAddressesClient, 
            IUtility utility) : base(logger)
        {
            _employerVacancyClient = employerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _getAddressesClient = getAddressesClient;
            _utility = utility;
        }

        public async Task<VacancyEmployerInfoModel> GetVacancyEmployerInfoModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Employer_Post);

            var model = new VacancyEmployerInfoModel() {
                VacancyId = vacancy.Id,
                EmployerAccountId = vrm.EmployerAccountId,
                AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId
            };

            if (vacancy.EmployerNameOption.HasValue)
            {
                model.EmployerIdentityOption = vacancy.EmployerNameOption.Value.ConvertToModelOption();
                model.AnonymousName = vacancy.IsAnonymous ? vacancy.EmployerName : null;
                model.AnonymousReason = vacancy.IsAnonymous ? vacancy.AnonymousReason : null;
            }
                
            return model;
        }

        public async Task<LocationViewModel> GetLocationViewModelAsync(
            VacancyRouteModel vrm, VacancyEmployerInfoModel employerInfoModel, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.Location_Get);

            var accountLegalEntityPublicHashedId = employerInfoModel?.AccountLegalEntityPublicHashedId != null
                ? employerInfoModel.AccountLegalEntityPublicHashedId
                : vacancy.AccountLegalEntityPublicHashedId;

            var vm = new LocationViewModel();
            vm.VacancyId = vrm.VacancyId;
            vm.EmployerAccountId = vrm.EmployerAccountId;
            vm.PageInfo = _utility.GetPartOnePageInfo(vacancy);
            
            vm.IsAnonymousVacancy = (employerInfoModel?.EmployerIdentityOption == null) 
                ? vacancy.IsAnonymous 
                : employerInfoModel.EmployerIdentityOption == EmployerIdentityOption.Anonymous;

            var employerProfile =
                await _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, accountLegalEntityPublicHashedId);

            var allLocations = await GetAllAvailableLocationsAsync(employerProfile);
            
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

        public async Task<GetAddressesListResponse> GetAddresses(string searchTerm)
        {
            var addresses = await _getAddressesClient.GetAddresses(searchTerm);
            return addresses;
        }

        public async Task<OrchestratorResponse> PostLocationEditModelAsync(
            LocationEditModel locationEditModel, VacancyEmployerInfoModel employerInfoModel, VacancyUser user)
        {            
            if (string.IsNullOrEmpty(locationEditModel.SelectedLocation))
                return new OrchestratorResponse(false);

            var vacancyTask = _utility.GetAuthorisedVacancyForEditAsync(locationEditModel, RouteNames.Employer_Post);
            var employerVacancyInfoTask =
                _employerVacancyClient.GetEditVacancyInfoAsync(locationEditModel.EmployerAccountId);

            await Task.WhenAll(vacancyTask, employerVacancyInfoTask);

            var vacancy = vacancyTask.Result;
            var editVacancyInfo = employerVacancyInfoTask.Result;

            var accountLegalEntityPublicHashedId = !string.IsNullOrEmpty(employerInfoModel?.AccountLegalEntityPublicHashedId)
                ? employerInfoModel.AccountLegalEntityPublicHashedId
                : vacancy.AccountLegalEntityPublicHashedId;

            var selectedOrganisation =
                    editVacancyInfo.LegalEntities.Single(l => l.AccountLegalEntityPublicHashedId == accountLegalEntityPublicHashedId);

            var employerProfile =
                await _recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, selectedOrganisation.AccountLegalEntityPublicHashedId);

            var allLocations = await GetAllAvailableLocationsAsync(employerProfile);

            var newLocation = 
                locationEditModel.SelectedLocation == LocationViewModel.UseOtherLocationConst 
                ? locationEditModel.ToAddressString() 
                : locationEditModel.SelectedLocation;

            var matchingAddress = GetMatchingAddress(newLocation, allLocations);

            var employerLocation = matchingAddress != null ? matchingAddress : ConvertToDomainAddress(locationEditModel);

            // this has diverged from the usual pattern because the individual properties are review fields
            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.EmployerLocation?.AddressLine1,
                FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine1),
                vacancy,
                (v) =>
                {
                    return employerLocation.AddressLine1;
                });

            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.EmployerLocation?.AddressLine2,
                FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine2),
                vacancy,
                (v) =>
                {
                    return employerLocation.AddressLine2;
                });

            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.EmployerLocation?.AddressLine3,
                FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine3),
                vacancy,
                (v) =>
                {
                    return employerLocation.AddressLine3;
                });

            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.EmployerLocation?.AddressLine4,
                FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine4),
                vacancy,
                (v) =>
                {
                    return employerLocation.AddressLine4;
                });

            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.EmployerLocation?.Postcode,
                FieldIdResolver.ToFieldId(v => v.EmployerLocation.Postcode),
                vacancy,
                (v) =>
                {
                    return employerLocation.Postcode;
                });

            vacancy.EmployerLocation = employerLocation;

            // if cookie is found then update legal entity and name option from cookie
            if (employerInfoModel != null)
            {
                SetVacancyWithEmployerReviewFieldIndicators(
                    vacancy.LegalEntityName,
                    FieldIdResolver.ToFieldId(v => v.EmployerName),
                    vacancy,
                    (v) =>
                    {
                        return v.LegalEntityName = selectedOrganisation.Name;
                    });

                SetVacancyWithEmployerReviewFieldIndicators(
                    vacancy.EmployerNameOption,
                    FieldIdResolver.ToFieldId(v => v.EmployerName),
                    vacancy,
                    (v) =>
                    {
                        return v.EmployerNameOption = employerInfoModel.EmployerIdentityOption?.ConvertToDomainOption();
                    });

                if (employerInfoModel.EmployerIdentityOption == EmployerIdentityOption.NewTradingName)
                {
                    SetVacancyWithEmployerReviewFieldIndicators(
                        employerProfile.TradingName,
                        FieldIdResolver.ToFieldId(v => v.EmployerName),
                        vacancy,
                        (e) =>
                        {
                            // the indicator will be set for the vacancy when the employer profile will change to the new trading name
                            return employerInfoModel.NewTradingName;
                        });
                }

                vacancy.AccountLegalEntityPublicHashedId = selectedOrganisation.AccountLegalEntityPublicHashedId;
                vacancy.AnonymousReason = vacancy.IsAnonymous ? employerInfoModel.AnonymousReason : null;
                vacancy.EmployerName = vacancy.IsAnonymous ? employerInfoModel.AnonymousName : null;
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

        private Address GetMatchingAddress(string locationToMatch, IEnumerable<Address> allLocations)
        {
            var matchingLocation =
                allLocations
                    .FirstOrDefault(l => l.ToAddressString().Equals(locationToMatch, StringComparison.OrdinalIgnoreCase));
            return matchingLocation;
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
            if (string.IsNullOrEmpty(employerProfile.AccountLegalEntityPublicHashedId) && !string.IsNullOrEmpty(employerInfoModel?.AccountLegalEntityPublicHashedId)) 
            {
                updateProfile = true;
                employerProfile.AccountLegalEntityPublicHashedId = employerInfoModel.AccountLegalEntityPublicHashedId;
            }
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

        private async Task<List<Address>> GetAllAvailableLocationsAsync(EmployerProfile employerProfile)
        {
            var employerData = await _employerVacancyClient.GetEditVacancyInfoAsync(employerProfile.EmployerAccountId);
            var legalEntity = employerData.LegalEntities.First(l => l.AccountLegalEntityPublicHashedId == employerProfile.AccountLegalEntityPublicHashedId);
            
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