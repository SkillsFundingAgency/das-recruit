using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
 
namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Models
{
    public class ApprenticeshipProgramme : IApprenticeshipProgramme
    {
        public string Id { get; set; }

        public TrainingType ApprenticeshipType { get; set; }

        public string Title { get; set; }

        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public ProgrammeLevel Level { get; set; }

        public int Duration { get; set; }

        public bool IsActive { get; set; }
    }
}
