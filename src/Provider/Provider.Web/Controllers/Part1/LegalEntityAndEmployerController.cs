using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class LegalEntityandEmployerController : EmployerControllerBase
    {
        private readonly LegalEntityAndEmployerOrchestrator _orchestrator;
        private readonly IFeature _feature;
        private readonly ServiceParameters _serviceParameters;

        public LegalEntityandEmployerController(
            LegalEntityAndEmployerOrchestrator orchestrator, 
            IHostingEnvironment hostingEnvironment, 
            IFeature feature,
            ServiceParameters serviceParameters)
            : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _feature = feature;
            _serviceParameters = serviceParameters;
        }

        [HttpGet("legal-entity-employer", Name = RouteNames.LegalEntityEmployer_Get)]
        public async Task<IActionResult> LegalEntityAndEmployer(VacancyRouteModel vrm, [FromQuery]string searchTerm, [FromQuery]int? page, [FromQuery] string wizard = "true")
        {
            var info = GetVacancyEmployerInfoCookie(vrm.VacancyId.GetValueOrDefault());

            var vm = await _orchestrator.GetLegalEntityAndEmployerViewModelAsync(vrm, User.GetUkprn(), searchTerm, page);

            if (info == null || !string.IsNullOrEmpty(info.AccountLegalEntityPublicHashedId))
            {
                SetVacancyEmployerInfoCookie(vm.VacancyEmployerInfoModel);
            }
            else
            {
                if (!string.IsNullOrEmpty(info.AccountLegalEntityPublicHashedId))
                {
                    vm.SelectedOrganisationId = info.AccountLegalEntityPublicHashedId;    
                }
            }

            if (vm.HasOnlyOneOrganisation)
            {
                var model = new LegalEntityAndEmployerEditModel();
                {
                    model.SelectedOrganisationId = vm.Organisations.FirstOrDefault()?.Id;
                }
                await _orchestrator.SetAccountLegalEntityPublicId(vrm,model, User.ToVacancyUser());

                return RedirectToRoute(_serviceParameters.VacancyType == VacancyType.Traineeship 
                    ? RouteNames.TraineeSector_Get 
                    : RouteNames.Training_Get, new {Wizard = wizard, vrm.Ukprn, vrm.VacancyId});
            }

            return View(vm);
        }

        [HttpPost("legal-entity-employer", Name = RouteNames.LegalEntityEmployer_Post)]
        public async Task<IActionResult> LegalEntityAndEmployer(LegalEntityAndEmployerEditModel m, [FromQuery] bool wizard)
        {

            var vm = await _orchestrator.GetLegalEntityAndEmployerViewModelAsync(m, User.GetUkprn(), m.SearchTerm, m.Page);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            await _orchestrator.SetAccountLegalEntityPublicId(new VacancyRouteModel
            {
                Ukprn = m.Ukprn,
                VacancyId = m.VacancyId
            }, m, HttpContext.User.ToVacancyUser());

            
            return RedirectToRoute(RouteNames.EmployerName_Get, new {Wizard = wizard, m.Ukprn, m.VacancyId});
        }

    }
}