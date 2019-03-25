using Esfa.Recruit.Employer.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Models
{
    public class EmployerInfoModel
    {
        public long? LegalEntityId { get; set; }
        public EmployerNameOptionViewModel? EmployerNameOption { get; set; }
        public string NewTradingName { get; set; }        
    }
}