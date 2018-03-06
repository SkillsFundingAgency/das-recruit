using System;

namespace Esfa.Recruit.Vacancies.Jobs.TrainingTypes.Models
{
    public class ApprenticeshipProgramme
    {
        public string Id { get; internal set; }
        public TrainingType ApprenticeshipType { get; internal set; }
        public string Title { get; internal set; }
        public DateTime? EffectiveFrom { get; internal set; }
        public DateTime? EffectiveTo { get; internal set; }
        public int Level { get; internal set; }
        public int Duration { get; internal set; }
    }
}
