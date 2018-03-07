namespace Esfa.Recruit.Employer.Web.ViewModels.Preview
{
    public class IndexViewModel
    {
        public string Title { get; set; }        
        public long? Ukprn { get; internal set; }
        public string ProviderName { get; set; }
        public string ProviderAddress { get; set; }


        public bool HasTrainingProviderDetails => Ukprn.HasValue;
        public bool CanSubmit { get; set; }
    }
}
