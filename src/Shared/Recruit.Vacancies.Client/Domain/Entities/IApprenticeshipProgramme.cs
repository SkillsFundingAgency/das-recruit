using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public interface IApprenticeshipProgramme
{
    string Id { get; }
    TrainingType ApprenticeshipType { get; }
    string Title { get; }
    DateTime? EffectiveFrom { get; }
    DateTime? EffectiveTo { get; }
    DateTime? LastDateStarts { get; }
    ApprenticeshipLevel ApprenticeshipLevel { get; }
    int Duration { get; }
    bool IsActive { get; set; }
    int? EducationLevelNumber { get; set; }
}