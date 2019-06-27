namespace Esfa.Recruit.Employer.Web.Models
{
    public enum SelectTrainingProviderResponseType
    {
        NotFound,
        Continue,
        Confirm
    }

    public class PostSelectTrainingProviderResult
    {
        public long? FoundProviderUkprn { get; set; }

        public SelectTrainingProviderResponseType ResponseType { get; set; }
    }
}
