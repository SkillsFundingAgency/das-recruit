using System;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.EmployerName;

namespace Esfa.Recruit.Provider.Web.Models
{
    public class VacancyEmployerInfoModel
    {
        public Guid? VacancyId { get; set; }
        public long? LegalEntityId { get; set; }
        public EmployerNameOptionViewModel? EmployerNameOption { get; set; }
        public string NewTradingName { get; set; }
        public bool HasLegalEntityChanged { get; set; }
    }
}