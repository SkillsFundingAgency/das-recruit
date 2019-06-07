using System;
using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.NumberOfPositions
{
    public class NumberOfPositionsViewModel : VacancyRouteModel
    {
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(NumberOfPositions)
        };
        public PartOnePageInfoViewModel PageInfo { get; set; }
        public string NumberOfPositions { get; set; }
    }
}
