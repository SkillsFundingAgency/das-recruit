using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public interface IApprenticeshipProgramme
    {
        string Id { get; }
        TrainingType ApprenticeshipType { get; }
        string Title { get; }
        DateTime? EffectiveFrom { get; }
        DateTime? EffectiveTo { get; }
        ProgrammeLevel Level { get; }
        int Duration { get; }
        bool IsActive { get; set; }
    }
}