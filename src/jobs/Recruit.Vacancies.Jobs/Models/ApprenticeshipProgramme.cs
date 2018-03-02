using System;

namespace Esfa.Recruit.Vacancies.Jobs.Models
{
    public class ApprenticeshipProgramme
    {
        public string Id { get; internal set; }
        public ApprenticeshipType ApprenticeshipType { get; internal set; }
        public string Title { get; internal set; }
        public DateTime? EffectiveFrom { get; internal set; }
        public DateTime? EffectiveTo { get; internal set; }
        public int Level { get; internal set; }
    }
}
