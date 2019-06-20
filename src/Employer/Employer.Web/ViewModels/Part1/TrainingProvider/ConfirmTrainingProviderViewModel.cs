using System;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider
{
    public class ConfirmTrainingProviderViewModel
    {
        public string EmployerAccountId { get; set; }
        public Guid VacancyId { get; set; }

        public string Title { get; set; }
        public string ProviderName { get; set; }
        public string ProviderAddress { get; set; }
        public long Ukprn { get; set; }
        public PartOnePageInfoViewModel PageInfo { get; set; }
    }
}