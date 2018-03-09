using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Projections
{
    public class ApprenticeshipProgramme
    {
        public string Id { get; set; }
        public TrainingType ApprenticeshipType { get; set; }
        public string Title { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int Level { get; set; }
        public int Duration { get; set; }
    }
}
