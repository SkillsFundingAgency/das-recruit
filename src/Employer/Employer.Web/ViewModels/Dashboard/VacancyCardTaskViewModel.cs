using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Dashboard;

public record VacancyCardTaskViewModel
{
    public string Text { get; set; }
    public FilteringOptions Filter { get; set; }
    public bool Show { get; set; }
}