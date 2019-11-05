using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    public class ApprenticeshipProgramme : IApprenticeshipProgramme
    {
        public string Id { get; set; }

        public TrainingType ApprenticeshipType { get; set; }

        public string Title { get; set; }

        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public ApprenticeshipLevel ApprenticeshipLevel { get; set; }

        public int Duration { get; set; }

        public bool IsActive { get; set; }

        public int? EducationLevelNumber { get; set; }
    }
}