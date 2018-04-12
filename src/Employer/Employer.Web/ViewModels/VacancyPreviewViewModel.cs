using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Humanizer;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class VacancyPreviewViewModel : DisplayVacancyViewModel
    {
        public bool HasTrainingProviderDetails => !string.IsNullOrEmpty(ProviderName);

        public bool HasContactDetails =>    !string.IsNullOrEmpty(ContactName) 
                                            && !string.IsNullOrEmpty(ContactEmail)
                                            && !string.IsNullOrEmpty(ContactTelephone);

        public IList<string> OrderedFieldNames => new List<string>
        {
            // This list is incomplete
            nameof(VacancyDescription),
            nameof(TrainingDescription),
            nameof(OutcomeDescription)
        };
    }
}
