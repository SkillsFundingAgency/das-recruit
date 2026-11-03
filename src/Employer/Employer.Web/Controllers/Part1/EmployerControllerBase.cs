using System;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1;

public abstract class EmployerControllerBase(IWebHostEnvironment hostingEnvironment) : Controller
{
    protected void SetVacancyEmployerInfoCookie(VacancyEmployerInfoModel model)
    {
        var info = JsonConvert.SerializeObject(model);
        Response.Cookies.SetSessionCookie(hostingEnvironment, CookieNames.VacancyEmployerInfo, info);
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
        Response.Cookies.DeleteSessionCookie(hostingEnvironment, CookieNames.VacancyEmployerInfo);
    }

    protected IActionResult CancelAndRedirect(bool wizard, VacancyRouteModel vrm)
    {
        DeleteVacancyEmployerInfoCookie();
        return wizard 
            ? RedirectToRoute(RouteNames.VacanciesGetAll, new {vrm.EmployerAccountId}) 
            : RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {vrm.VacancyId, vrm.EmployerAccountId});
    }
}