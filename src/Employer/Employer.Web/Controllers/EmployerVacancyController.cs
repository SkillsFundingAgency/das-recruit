using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class EmployerVacancyController : Controller
    {
        private readonly EmployerVacancyOrchestrator _employerVacancyOrchestrator;

        public EmployerVacancyController(EmployerVacancyOrchestrator employerVacancyOrchestrator)
        {
            _employerVacancyOrchestrator = employerVacancyOrchestrator;
        }

        [HttpGet("employer-create-vacancy", Name = RouteNames.EmployerCreateVacancy_Get)]        
        public async Task<IActionResult> CreateVacancy([FromRoute] string employerAccountId,[FromQuery]string ukprn, [FromQuery]string programmeId)
        {
            TempData[TempDataKeys.ReferredFromMa] = true;

            var providerTask = _employerVacancyOrchestrator.GetProviderUkprn(ukprn);
            var programmeTask = _employerVacancyOrchestrator.GetProgrammeId(programmeId);

            await Task.WhenAll(providerTask, programmeTask);

            var provider = providerTask.Result;
            var programme = programmeTask.Result;

            ManageTempData(provider, programme);

            if (provider == null && programme == null)
            {
                if (await _employerVacancyOrchestrator.HasNoVacancies(employerAccountId))
                    return RedirectToRoute(RouteNames.Dashboard_Get);
                return RedirectToRoute(RouteNames.CreateVacancyOptions_Get);
            }
            return RedirectToRoute(RouteNames.CreateVacancy_Get);
        }

        private void ManageTempData(TrainingProviderSummary provider, IApprenticeshipProgramme programme)
        {
            if (provider != null)
                TempData[TempDataKeys.ReferredUkprn] = provider.Ukprn;
            else
            {
                TempData.Remove(TempDataKeys.ReferredUkprn);
            }
            
            if (programme != null)
                TempData[TempDataKeys.ReferredProgrammeId] = programme.Id;
            else
            {
                TempData.Remove(TempDataKeys.ReferredProgrammeId);
            }
        }

        [HttpGet("employer-manage-vacancy", Name = RouteNames.EmployerManageVacancy_Get)]
        public IActionResult ManageVacancy()
        {
            TempData[TempDataKeys.ReferredFromMa] = true;
            return RedirectToRoute(RouteNames.Vacancies_Get);
        }        
    }
}