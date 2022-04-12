using System;
using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.ShortDescription
{
    public class ShortDescriptionViewModel 
    {
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public Guid VacancyId { get; set; }
        public string ShortDescription { get; set; }
        
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ShortDescription)
        };

        public string Title { get; set; }
    }
}