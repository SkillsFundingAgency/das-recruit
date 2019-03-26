using System;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    public abstract class EmployerControllerBase : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private string GetKey(Guid vacancyId, string fieldName) => string.Format("{0}_{1}", fieldName, vacancyId);
        protected EmployerControllerBase(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        protected void SetVacancyEmployerInfoCookie(Guid vacancyId, long legalEntityId)
        {
            var info = JsonConvert.SerializeObject(new VacancyEmployerInfoModel { VacancyId = vacancyId, LegalEntityId = legalEntityId });
            Response.Cookies.SetSessionCookie(_hostingEnvironment, CookieNames.VacancyEmployerInfo , info);
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


    }    
}