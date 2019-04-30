using System;
using Esfa.Recruit.Shared.Web.Models;

namespace Esfa.Recruit.Provider.Web.Model
{
    public class VacancyEmployerInfoModel
    {
        public Guid? VacancyId { get; set; }
        public long? LegalEntityId { get; set; }
        public EmployerIdentityOption? EmployerIdentityOption { get; set; }
        public string NewTradingName { get; set; }
        public bool HasLegalEntityChanged { get; set;}
        public string AnonymousName { get; set; }
        public string AnonymousReason { get; set; }
    }
}