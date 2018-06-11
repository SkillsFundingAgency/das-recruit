using Esfa.Recruit.Employer.Web.Views;
using System;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ConfirmTrainingProviderViewModel
    {
        // Cannot inherit from VacancyRouteModel because this is a pure output viewmodel not produced from MVC modelbinder.
        public string EmployerAccountId { get; set; }
        public Guid VacancyId { get; set; }

        public string Title { get; set; }
        public string ProviderName { get; set; }
        public string ProviderAddress { get; set; }
        public long Ukprn { get; set; }
        public static string PreviewSectionAnchor => PreviewAnchors.TrainingProviderSection;
    }
}