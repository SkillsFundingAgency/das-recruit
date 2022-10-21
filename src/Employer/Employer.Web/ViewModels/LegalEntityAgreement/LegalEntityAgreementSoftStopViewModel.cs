using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.LegalEntityAgreement
{
    public class LegalEntityAgreementSoftStopViewModel
    {
        public bool HasLegalEntityAgreement { get; set; }
        public string LegalEntityName { get; set; }

        public PartOnePageInfoViewModel PageInfo { get; set; }
    }
}
