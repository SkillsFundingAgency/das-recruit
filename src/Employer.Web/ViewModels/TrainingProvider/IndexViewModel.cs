﻿namespace Esfa.Recruit.Employer.Web.ViewModels.TrainingProvider
{
    public class IndexViewModel
    {
        public string Title { get; set; }
        public long? Ukprn { get; set; }
        public string ProviderName { get; set; }
        public string ProviderAddress { get; set; }

        public bool HasSelectedTrainingProvider => Ukprn.HasValue;
    }
}
