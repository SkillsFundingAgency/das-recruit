using Esfa.Recruit.Employer.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewComponents;

public class VacancyCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(VacancyCardViewModel vm) => View(vm);
}