using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.VacancyDescription
{
    public class VacancyDescriptionViewModel
    {
        public string Title { get; internal set; }
        public string VacancyDescription { get; internal set; }
        public string TrainingDescription { get; internal set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(VacancyDescriptionEditModel.VacancyDescription),
            nameof(VacancyDescriptionEditModel.TrainingDescription),
        };
    }
}