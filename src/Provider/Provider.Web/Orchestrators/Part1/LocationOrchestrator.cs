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
using Esfa.Recruit.Provider.Web.ViewModels.Part1.EmployerName;

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

            return await GetViewModelAsync(vacancy, employerInfoModel?.HasLegalEntityChanged, employerInfoModel?.LegalEntityId);
        }
        private async Task<LocationViewModel> GetViewModelAsync(Vacancy vacancy, bool? hasLegalEntityChanged, long? selectedOrganisationId)        
        {
            var employerData = await _providerVacancyClient.GetProviderEmployerVacancyDataAsync(vacancy.TrainingProvider.Ukprn.GetValueOrDefault(), vacancy.EmployerAccountId);

            var legalEntityId = selectedOrganisationId.HasValue ? selectedOrganisationId : vacancy.LegalEntityId; 

            var legalEntity = employerData.LegalEntities.First(l => l.LegalEntityId == legalEntityId);
            if (legalEntity == null)
                throw new ArgumentNullException("Legal entity is required for location");

            var vm = new LocationViewModel();
            vm.PageInfo = Utility.GetPartOnePageInfo(vacancy);

            vm.LegalEntityLocation = legalEntity.Address.ToString();

            if (vacancy.EmployerLocation != null && (!hasLegalEntityChanged.HasValue || hasLegalEntityChanged == false))
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
            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetLocationFieldIndicators());
            }
            return vm;
        } 

        public async Task<VacancyEmployerInfoModel> GetVacancyEmployerInfoModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Employer_Post);

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
            LocationEditModel m, VacancyUser user, long ukprn, VacancyEmployerInfoModel employerInfoModel)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_providerVacancyClient, 
                _recruitVacancyClient, m, RouteNames.Location_Post);
            
            var employerVacancyInfo = await _providerVacancyClient.GetProviderEmployerVacancyDataAsync(ukprn, vacancy.EmployerAccountId);

            LegalEntity selectedOrganisation;
            //if cookie is found update legal entity and name option
            if(employerInfoModel != null)
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
                async v => 
                {
                    if (employerInfoModel.EmployerNameOption == EmployerNameOptionViewModel.NewTradingName)
                    {
                        await UpdateEmployerProfileAsync(vacancy.EmployerAccountId, 
                            employerInfoModel.LegalEntityId.GetValueOrDefault(), 
                            employerInfoModel.NewTradingName, user);
                    }
                    await _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                });
        }

        private async Task UpdateEmployerProfileAsync(string employerAccountId, long legalEntityId, string tradingName, VacancyUser user)
        {
            var employerProfile =
                await _recruitVacancyClient.GetEmployerProfileAsync(employerAccountId, legalEntityId);

            if (employerProfile == null)
            {
                throw new NullReferenceException($"No Employer Profile was found for employerAccount: {employerAccountId}, legalEntity: {legalEntityId}");
            }

            if (employerProfile.TradingName != tradingName)
            {
                employerProfile.TradingName = tradingName;
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
    }
}