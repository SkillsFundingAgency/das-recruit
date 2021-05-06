namespace Esfa.Recruit.Provider.Web.Models
{
    public class SubmitVacancyResponse
    {
        public bool HasProviderAgreement { get; set; }
        public bool HasLegalEntityAgreement { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsReviewed { get; set; }
    }
}
