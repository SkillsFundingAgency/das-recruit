using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ApplicationProcessViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }
        public ApplicationMethod? ApplicationMethod { get; internal set; }
        public string ApplicationInstructions { get; internal set; }
        public string ApplicationUrl { get; internal set; }

        public bool HasEmptyApplicationMethod => !ApplicationMethod.HasValue;
        public ReviewSummaryViewModel Review { get; set; } = new();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ApplicationProcessEditModel.ApplicationMethod),
            nameof(ApplicationProcessEditModel.ApplicationUrl),
            nameof(ApplicationProcessEditModel.ApplicationInstructions)
        };

        public string FindAnApprenticeshipUrl { get; internal set; }
        public bool IsTaskListCompleted { get ; set ; }
    }
}
