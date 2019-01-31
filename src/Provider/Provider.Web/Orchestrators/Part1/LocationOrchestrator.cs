using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Location;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class LocationOrchestrator : EntityValidatingOrchestrator<Vacancy, LocationEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerName | VacancyRuleSet.EmployerAddress;
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        public LocationOrchestrator(IProviderVacancyClient providerVacancyClient, 
            IRecruitVacancyClient recruitVacancyClient, ILogger<LocationOrchestrator> logger)
            : base(logger)
        {
            _providerVacancyClient = providerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
        }
        public async Task<LocationViewModel> GetLocationViewModelAsync(VacancyRouteModel vrm, long ukprn)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(
                _providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.Location_Get);

            var vm = new LocationViewModel
            {
                LegalEntities = await GetLegalEntityViewModelsAsync(ukprn, vacancy.EmployerAccountId),
                SelectedLegalEntityId = vacancy.LegalEntityId,
                PageInfo = Utility.GetPartOnePageInfo(vacancy)
            };

            if (vacancy.EmployerLocation != null)
            {
                vm.AddressLine1 = vacancy.EmployerLocation.AddressLine1;
                vm.AddressLine2 = vacancy.EmployerLocation.AddressLine2;
                vm.AddressLine3 = vacancy.EmployerLocation.AddressLine3;
                vm.AddressLine4 = vacancy.EmployerLocation.AddressLine4;
                vm.Postcode = vacancy.EmployerLocation.Postcode;
            }
            else if (vm.LegalEntities.Count() == 1 && vacancy.EmployerLocation == null)
            {
                var defaultLegalEntity = vm.LegalEntities.First();
                vm.AddressLine1 = defaultLegalEntity.Address.AddressLine1;
                vm.AddressLine2 = defaultLegalEntity.Address.AddressLine2;
                vm.AddressLine3 = defaultLegalEntity.Address.AddressLine3;
                vm.AddressLine4 = defaultLegalEntity.Address.AddressLine4;
                vm.Postcode = defaultLegalEntity.Address.Postcode;
            }

            // if (vacancy.Status == VacancyStatus.Referred)
            // {
            //     vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
            //         ReviewFieldMappingLookups.GetEmployerFieldIndicators());
            // }

            return vm;
        }

        public async Task<LocationViewModel> GetLocationViewModelAsync(LocationEditModel m, long ukprn)
        {
            var vm = await GetLocationViewModelAsync((VacancyRouteModel)m, ukprn);

            vm.SelectedLegalEntityId = m.SelectedLegalEntityId;
            vm.AddressLine1 = m.AddressLine1;
            vm.AddressLine2 = m.AddressLine2;
            vm.AddressLine3 = m.AddressLine3;
            vm.AddressLine4 = m.AddressLine4;
            vm.Postcode = m.Postcode;

            return vm;
        }

        public async Task<OrchestratorResponse> PostLocationEditModelAsync(LocationEditModel m, VacancyUser user, long ukprn)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_providerVacancyClient, _recruitVacancyClient, m, RouteNames.Employer_Post);
            
            var employerVacancyInfo = await _providerVacancyClient.GetProviderEmployerVacancyDataAsync(ukprn, vacancy.EmployerAccountId);

            var selectedLegalEntity = employerVacancyInfo.LegalEntities.SingleOrDefault(x => x.LegalEntityId == m.SelectedLegalEntityId);

            vacancy.LegalEntityId = m.SelectedLegalEntityId;

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

            return await ValidateAndExecute(
                vacancy, 
                v => _recruitVacancyClient.Validate(v, ValidationRules),
                v => _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user));
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, LocationEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, LocationEditModel>();

            mappings.Add(e => e.EmployerName, vm => vm.SelectedLegalEntityId);
            mappings.Add(e => e.EmployerLocation.AddressLine1, vm => vm.AddressLine1);
            mappings.Add(e => e.EmployerLocation.AddressLine2, vm => vm.AddressLine2);
            mappings.Add(e => e.EmployerLocation.AddressLine3, vm => vm.AddressLine3);
            mappings.Add(e => e.EmployerLocation.AddressLine4, vm => vm.AddressLine4);
            mappings.Add(e => e.EmployerLocation.Postcode, vm => vm.Postcode);

            return mappings;

        }

        private LegalEntityViewModel MapLegalEntitiesToOrgs(LegalEntity data)
        {
            return new LegalEntityViewModel { Id = data.LegalEntityId.ToString(), Name = data.Name, Address = data.Address };
        }

        private async Task<List<LegalEntityViewModel>> GetLegalEntityViewModelsAsync(long ukprn, string employerAccountId)
        {
            var result = new List<LegalEntityViewModel>();

            var info = await _providerVacancyClient.GetProviderEmployerVacancyDataAsync(ukprn, employerAccountId);  

            if (info == null || !info.LegalEntities.Any())
            {
                Logger.LogError("No legal entities found for {employerAccountId}", employerAccountId);
                return null; // TODO: Can we carry on without a list of legal entities.
            }

            return info.LegalEntities.Select(MapLegalEntitiesToOrgs).ToList();
        }
    }
}