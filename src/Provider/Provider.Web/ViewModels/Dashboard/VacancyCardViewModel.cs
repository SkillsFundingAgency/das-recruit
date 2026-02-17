using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Dashboard;

public record VacancyCardViewModel
{
    public int Count { get; set; }
    public string HeadingText { get; set; }
    public string Description { get; set; }
    public string RouteName { get; set; }
    public long Ukprn { get; set; }
    public FilteringOptions Filter { get; set; }

    public IReadOnlyCollection<VacancyCardTaskViewModel> Tasks { get; set; } = [];
}