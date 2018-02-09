using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Employer.Web.ViewModels.CandidateProfile;

namespace Employer.Web.Controllers
{
    public class CandidateProfileController : Controller
    {
        [HttpGet, Route("candidate-profile")]
        public IActionResult Index()
        {
            var vm = new IndexViewModel
            {
                Title = Dummy.VacancyTitle
            };
            return View(vm);
        }

        [HttpPost, Route("candidate-profile")]
        public IActionResult Index(IndexViewModel vm)
        {
            return RedirectToAction("Index", "ApprenticeshipDetails");
        }
    }
}