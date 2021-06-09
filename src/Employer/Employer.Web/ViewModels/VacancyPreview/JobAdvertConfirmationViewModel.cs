using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview
{
    public class JobAdvertConfirmationViewModel
    {
        public string Title { get; set; }
        public string VacancyReference { get; set; }
        public bool ApprovedJobAdvert { get; set; }
        public bool RejectedJobAdvert { get; set; }        
        public string TrainingProviderName { get; set; }
        public bool HasVacancyReference => !string.IsNullOrEmpty(VacancyReference);
        public string FindAnApprenticeshipUrl { get; set; }
    }
}
