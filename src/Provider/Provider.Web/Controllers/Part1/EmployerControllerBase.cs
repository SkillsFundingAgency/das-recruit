using System;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Mappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    public class EmployerControllerBase : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        protected EmployerControllerBase(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        protected void SetVacancyEmployerInfoCookie(VacancyEmployerInfoModel model)
        {
            var info = JsonConvert.SerializeObject(model);
            Response.Cookies.SetSessionCookie(_hostingEnvironment, CookieNames.VacancyEmployerInfo, info);
        }

        protected VacancyEmployerInfoModel GetVacancyEmployerInfoCookie(Guid vacancyId)
        {
            var value = Request.Cookies.GetCookie(CookieNames.VacancyEmployerInfo);
            if (value != null) 
            {
                var info = JsonConvert.DeserializeObject<VacancyEmployerInfoModel>(value);
                if(info.VacancyId == vacancyId) return info;
            }
            return null;
        }

        protected void DeleteVacancyEmployerInfoCookie()
        {
            Response.Cookies.DeleteSessionCookie(_hostingEnvironment, CookieNames.VacancyEmployerInfo);
        }

        protected IActionResult CancelAndRedirect(bool wizard, VacancyRouteModel vacancyRouteModel)
        {
            DeleteVacancyEmployerInfoCookie();
            return wizard 
                ? RedirectToRoute(RouteNames.Vacancies_Get, new {vacancyRouteModel.Ukprn, vacancyRouteModel.VacancyId}) 
                : RedirectToRoute(RouteNames.Vacancy_Preview_Get, new {vacancyRouteModel.Ukprn, vacancyRouteModel.VacancyId}, Anchors.AboutEmployerSection);
        }
    }
}