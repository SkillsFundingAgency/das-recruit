using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Address = Esfa.Recruit.Vacancies.Client.Domain.Entities.Address;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class LocationOrchestrator(
        IProviderVacancyClient providerVacancyClient,
        IRecruitVacancyClient recruitVacancyClient,
        ILogger<LocationOrchestrator> logger,
        IGetAddressesClient getAddressesClient,
        IUtility utility)
        : VacancyValidatingOrchestrator<LocationEditModel>(logger)
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerAddress;

        public async Task<OrchestratorResponse> PostLocationEditModelAsync(
            LocationEditModel locationEditModel, VacancyUser user, long ukprn,
            VacancyEmployerInfoModel employerInfoModel)
        {
            if (string.IsNullOrEmpty(locationEditModel.SelectedLocation))
                return new OrchestratorResponse(false);

            var vacancy = await utility.GetAuthorisedVacancyForEditAsync(locationEditModel, RouteNames.Location_Post);
            var accountLegalEntityPublicHashedId = !string.IsNullOrEmpty(employerInfoModel?.AccountLegalEntityPublicHashedId)
                ? employerInfoModel.AccountLegalEntityPublicHashedId : vacancy.AccountLegalEntityPublicHashedId;

            var employerVacancyInfoTask = providerVacancyClient.GetProviderEmployerVacancyDataAsync(ukprn, vacancy.EmployerAccountId);
            var employerProfileTask = recruitVacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, accountLegalEntityPublicHashedId);
            await Task.WhenAll(employerProfileTask, employerVacancyInfoTask);

            var employerVacancyInfo = employerVacancyInfoTask.Result;
            var employerProfile = employerProfileTask.Result;
            var selectedOrganisation = employerVacancyInfo.LegalEntities.Single(l => l.AccountLegalEntityPublicHashedId == accountLegalEntityPublicHashedId);
            var allLocations = await GetAllAvailableLocationsAsync(employerProfile, vacancy, ukprn);
            var newLocation =
                locationEditModel.SelectedLocation == LocationViewModel.UseOtherLocationConst
                    ? locationEditModel.ToAddressString()
                    : locationEditModel.SelectedLocation;

            var matchingAddress = GetMatchingAddress(newLocation, allLocations);

            var employerLocation = matchingAddress != null ? matchingAddress : ConvertToDomainAddress(locationEditModel);

            // this has diverged from the usual pattern because the individual properties are review fields
            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.EmployerLocation?.AddressLine1,
                FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine1),
                vacancy,
                (v) =>
                {
                    return employerLocation.AddressLine1;
                });

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.EmployerLocation?.AddressLine2,
                FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine2),
                vacancy,
                (v) =>
                {
                    return employerLocation.AddressLine2;
                });

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.EmployerLocation?.AddressLine3,
                FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine3),
                vacancy,
                (v) =>
                {
                    return employerLocation.AddressLine3;
                });

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.EmployerLocation?.AddressLine4,
                FieldIdResolver.ToFieldId(v => v.EmployerLocation.AddressLine4),
                vacancy,
                (v) =>
                {
                    return employerLocation.AddressLine4;
                });

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.EmployerLocation?.Postcode,
                FieldIdResolver.ToFieldId(v => v.EmployerLocation.Postcode),
                vacancy,
                (v) =>
                {
                    return employerLocation.Postcode;
                });

            vacancy.EmployerLocation = employerLocation;

            // if cookie is found update legal entity and name option
            if (employerInfoModel != null)
            {
                SetVacancyWithProviderReviewFieldIndicators(
                    vacancy.LegalEntityName,
                    FieldIdResolver.ToFieldId(v => v.EmployerName),
                    vacancy,
                    (v) =>
                    {
                        return v.LegalEntityName = selectedOrganisation.Name;
                    });

                SetVacancyWithProviderReviewFieldIndicators(
                    vacancy.EmployerNameOption,
                    FieldIdResolver.ToFieldId(v => v.EmployerName),
                    vacancy,
                    (v) =>
                    {
                        return v.EmployerNameOption = employerInfoModel.EmployerIdentityOption?.ConvertToDomainOption();
                    });

                if (employerInfoModel.EmployerIdentityOption == EmployerIdentityOption.NewTradingName)
                {
                    SetVacancyWithProviderReviewFieldIndicators(
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
                v => recruitVacancyClient.Validate(v, ValidationRules),
                async v =>
                {
                    await recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                    await UpdateEmployerProfileAsync(employerInfoModel, employerProfile, matchingAddress == null ? vacancy.EmployerLocation : null);
                });
        }

        public async Task<GetAddressesListResponse> GetAddresses(string searchTerm)
        {
            var addresses = await getAddressesClient.GetAddresses(searchTerm);
            return addresses;
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

        private async Task UpdateEmployerProfileAsync(VacancyEmployerInfoModel employerInfoModel,
            EmployerProfile employerProfile, Address address)
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
                await recruitVacancyClient.UpdateEmployerProfileAsync(employerProfile);
            }
        }

        private async Task<List<Address>> GetAllAvailableLocationsAsync(EmployerProfile employerProfile, Vacancy vacancy, long ukprn)
        {
            var providerData = await providerVacancyClient.GetProviderEditVacancyInfoAsync(ukprn);
            var employerInfo = providerData.Employers.Single(e => e.EmployerAccountId == vacancy.EmployerAccountId);
            var legalEntity = employerInfo.LegalEntities.First(l => l.AccountLegalEntityPublicHashedId == employerProfile.AccountLegalEntityPublicHashedId);
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