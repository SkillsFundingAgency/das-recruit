using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Employer.Web.ViewModels.NewVacancy;
using Esfa.Recruit.Employer.Web.Configuration.Routes;
using Esfa.Recruit.Storage.Client.Core.Messaging;
using Esfa.Recruit.Storage.Client.Core.Commands;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    public class NewVacancyController : Controller
    {
        private readonly IMessaging _messaging;

        public NewVacancyController(IMessaging messaging)
        {
            _messaging = messaging;
        }

        [HttpGet, Route("/new-vacancy", Name = RouteNames.NewVacancy_Index_Get)]
        public IActionResult Index()
        {
            var vm = new IndexViewModel();
            return View(vm);
        }


        [HttpPost, Route("/new-vacancy", Name = RouteNames.NewVacancy_Index_Post)]
        public IActionResult Index(IndexViewModel vm)
        {
            if(!ModelState.IsValid)
            {
                return View(vm);
            }

            //dummy code
            Dummy.VacancyTitle = vm.Title;

            var command = new CreateVacancyCommand
            {
                Title = vm.Title
            };

            _messaging.SendCommand(command);

            return RedirectToAction("Index", "Sections");
        }
        
    }
}