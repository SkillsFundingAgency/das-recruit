using System;
using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.ShortDescription
{
    public class ShortDescriptionViewModel : VacancyRouteModel
    {
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public string ShortDescription { get; set; }
        
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ShortDescription)
        };

        public string Title { get; set; }
        public bool IsTaskListCompleted { get; set; }
    }
}