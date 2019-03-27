using System;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Models
{
    public class VacancyEmployerInfoModel
    {
        public Guid? VacancyId { get; set; }
        public long? LegalEntityId { get; set; }
        public EmployerNameOptionViewModel? EmployerNameOption { get; set; }
        public string NewTradingName { get; set; }
        public bool HasLegalEntityChanged { get; set;}
    }
}