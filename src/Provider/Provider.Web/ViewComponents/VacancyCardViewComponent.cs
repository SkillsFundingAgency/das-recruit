using Esfa.Recruit.Provider.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.ViewComponents;

public class VacancyCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(VacancyCardViewModel vm) => View(vm);
}