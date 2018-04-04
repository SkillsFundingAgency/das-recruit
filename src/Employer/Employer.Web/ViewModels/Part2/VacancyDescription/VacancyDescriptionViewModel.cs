using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class VacancyDescriptionViewModel
    {
        public string Title { get; internal set; }
        public string VacancyDescription { get; internal set; }
        public string TrainingDescription { get; internal set; }
        public string OutcomeDescription { get; internal set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(VacancyDescriptionEditModel.VacancyDescription),
            nameof(VacancyDescriptionEditModel.TrainingDescription),
            nameof(VacancyDescriptionEditModel.OutcomeDescription)
        };
    }
}
