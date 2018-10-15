using System;

namespace Esfa.Recruit.Employer.Web.Models
{
    public class VacancyInfo
    {
        public Guid Id { get; internal set; }
        public string VacancyReference { get; internal set; }
        public string Title { get; internal set; }
    }
}