using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class EmployerOrchestrator : EntityValidatingOrchestrator<Vacancy, EmployerEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerName | VacancyRuleSet.EmployerAddress;
        private readonly IEmployerVacancyClient _client;
        private readonly IReviewSummaryService _reviewSummaryService;

        public EmployerOrchestrator(IEmployerVacancyClient client, ILogger<EmployerOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<EmployerViewModel> GetEmployerViewModelAsync(VacancyRouteModel vrm)
        {
            var getEmployerDataTask = _client.GetEditVacancyInfo(vrm.EmployerAccountId);
            var getVacancyTask = Utility.GetAuthorisedVacancyForEditAsync(_client, vrm, RouteNames.Employer_Get);

            await Task.WhenAll(getEmployerDataTask, getVacancyTask);

            var employerData = getEmployerDataTask.Result;
            var vacancy = getVacancyTask.Result;

            var vm = new EmployerViewModel
            {
                Organisations = BuildLegalEntityViewModels(employerData, vrm.EmployerAccountId),
                SelectedOrganisationId = vacancy.LegalEntityId,
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
            else if (employerData.LegalEntities.Count() == 1 && vacancy.EmployerLocation == null)
            {
                var defaultLegalEntity = employerData.LegalEntities.First();
                vm.AddressLine1 = defaultLegalEntity.Address.AddressLine1;
                vm.AddressLine2 = defaultLegalEntity.Address.AddressLine2;
                vm.AddressLine3 = defaultLegalEntity.Address.AddressLine3;
                vm.AddressLine4 = defaultLegalEntity.Address.AddressLine4;
                vm.Postcode = defaultLegalEntity.Address.Postcode;
            }

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModel(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetEmployerFieldIndicators());
            }

            return vm;
        }

        public async Task<EmployerViewModel> GetEmployerViewModelAsync(EmployerEditModel m)
        {
            var vm = await GetEmployerViewModelAsync((VacancyRouteModel)m);

            vm.SelectedOrganisationId = m.SelectedOrganisationId;
            vm.AddressLine1 = m.AddressLine1;
            vm.AddressLine2 = m.AddressLine2;
            vm.AddressLine3 = m.AddressLine3;
            vm.AddressLine4 = m.AddressLine4;
            vm.Postcode = m.Postcode;

            return vm;
        }

        public async Task<OrchestratorResponse> PostEmployerEditModelAsync(EmployerEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, m, RouteNames.Employer_Post);
            var employerVacancyInfo = await _client.GetEditVacancyInfo(m.EmployerAccountId);

            var selectedOrganisation = employerVacancyInfo.LegalEntities.SingleOrDefault(x => x.LegalEntityId == m.SelectedOrganisationId);

            vacancy.LegalEntityId = m.SelectedOrganisationId;
            vacancy.EmployerName = selectedOrganisation?.Name;

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
                v => _client.Validate(v, ValidationRules),
                v => _client.UpdateDraftVacancyAsync(vacancy, user)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, EmployerEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, EmployerEditModel>();

            mappings.Add(e => e.EmployerName, vm => vm.SelectedOrganisationId);
            mappings.Add(e => e.EmployerLocation.AddressLine1, vm => vm.AddressLine1);
            mappings.Add(e => e.EmployerLocation.AddressLine2, vm => vm.AddressLine2);
            mappings.Add(e => e.EmployerLocation.AddressLine3, vm => vm.AddressLine3);
            mappings.Add(e => e.EmployerLocation.AddressLine4, vm => vm.AddressLine4);
            mappings.Add(e => e.EmployerLocation.Postcode, vm => vm.Postcode);

            return mappings;
        }

        private LocationOrganisationViewModel MapLegalEntitiesToOrgs(LegalEntity data)
        {
            return new LocationOrganisationViewModel { Id = data.LegalEntityId.ToString(), Name = data.Name, Address = data.Address };
        }
        
        private IEnumerable<LocationOrganisationViewModel> BuildLegalEntityViewModels(EditVacancyInfo info, string employerAccountId)
        {
            if (info == null || !info.LegalEntities.Any())
            {
                Logger.LogError("No legal entities found for {employerAccountId}", employerAccountId);
                return null; // TODO: Can we carry on without a list of legal entities.
            }

            return info.LegalEntities.Select(MapLegalEntitiesToOrgs).ToList();
        }
    }
}
