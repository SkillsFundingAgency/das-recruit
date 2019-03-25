using System;
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
        protected EmployerControllerBase(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        protected void SetEmployerInfoCookie(Guid vacancyId, long legalEntityId)
        {
            var info = JsonConvert.SerializeObject(new EmployerInfoModel{LegalEntityId = legalEntityId});
            Response.Cookies.SetEmployerInfo(_hostingEnvironment, vacancyId, info);
        }

        protected void SetEmployerInfoCookie(Guid vacancyId, EmployerInfoModel model)
        {
            var info = JsonConvert.SerializeObject(model);
            Response.Cookies.SetEmployerInfo(_hostingEnvironment, vacancyId, info);
        }

        protected EmployerInfoModel GetEmployerInfoCookie(Guid vacancyId)
        {
            var value = Request.Cookies.GetEmployerInfo(vacancyId);
            if (value == null) return new EmployerInfoModel();
            return JsonConvert.DeserializeObject<EmployerInfoModel>(value);
        }

        protected void ClearCookie(Guid vacancyId)
        {
            Response.Cookies.ClearEmployerInfo(_hostingEnvironment, vacancyId);
        }


    }    
}