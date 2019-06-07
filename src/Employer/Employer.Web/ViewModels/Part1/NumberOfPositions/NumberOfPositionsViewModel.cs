using System;
using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.NumberOfPositions
{
    public class NumberOfPositionsViewModel 
    {
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(NumberOfPositions)
        };
        public PartOnePageInfoViewModel PageInfo { get; set; }
        public string NumberOfPositions { get; set; }
        [FromRoute]
        public Guid VacancyId { get; set; }
    }
}
