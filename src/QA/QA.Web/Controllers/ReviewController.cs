using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.Controllers
{
    [Route(RoutePrefixPaths.VacancyRoutePath)]
    public class ReviewController : Controller
    {
        [HttpGet("review", Name = RouteNames.Vacancy_Review_Get)]
        public IActionResult Review([FromRoute] string vacancyId) => View((object)vacancyId);
    }
}
