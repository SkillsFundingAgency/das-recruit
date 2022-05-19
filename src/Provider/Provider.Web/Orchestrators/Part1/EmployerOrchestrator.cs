using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
	public class EmployerOrchestrator 
    {
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IProviderRelationshipsService _providerRelationshipsService;
        private readonly ServiceParameters _serviceParameters;

        public EmployerOrchestrator(IProviderVacancyClient providerVacancyClient, IProviderRelationshipsService providerRelationshipsService, ServiceParameters serviceParameters)
        {
            _providerVacancyClient = providerVacancyClient;
            _providerRelationshipsService = providerRelationshipsService;
            _serviceParameters = serviceParameters;
        }

        public async Task<EmployersViewModel> GetEmployersViewModelAsync(VacancyRouteModel vrm)
        {
            var editVacancyInfo = await _providerVacancyClient.GetProviderEditVacancyInfoAsync(vrm.Ukprn);

            var employerInfos = editVacancyInfo?.Employers.ToList();
            if (editVacancyInfo?.Employers == null || employerInfos.Any() == false)
            {
                throw new MissingPermissionsException(string.Format(RecruitWebExceptionMessages.ProviderMissingPermission, vrm.Ukprn));
            }

            if (_serviceParameters.VacancyType.GetValueOrDefault() == VacancyType.Traineeship)
            {
                var providerPermissions = await _providerRelationshipsService.GetLegalEntitiesForProviderAsync(vrm.Ukprn, OperationType.Recruitment);
                employerInfos = employerInfos.Where(x =>
                        providerPermissions.FirstOrDefault(p => p.EmployerAccountId == x.EmployerAccountId) != null)
                    .ToList();

                if (!employerInfos.Any())
                {
                    throw new MissingPermissionsException(string.Format(RecruitWebExceptionMessages.ProviderMissingPermission, vrm.Ukprn));
                }
            }
            
            var vm = new EmployersViewModel
            {
                Employers = employerInfos.Select(e => new EmployerViewModel {Id = e.EmployerAccountId, Name = e.Name}),
                VacancyId = vrm.VacancyId,
                Ukprn = vrm.Ukprn
            };

            return vm;
        }
    }
}