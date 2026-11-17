using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;

public class ApprenticeshipProgramme : IApprenticeshipProgramme
{
    public string Id { get; set; }

    public TrainingType ApprenticeshipType { get; set; }

    public string Title { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public DateTime? LastDateStarts { get; set; }

    public ApprenticeshipLevel ApprenticeshipLevel { get; set; }

    public int Duration { get; set; }

    public bool IsActive { get; set; }

    public int? EducationLevelNumber { get; set; }

    public static implicit operator ApprenticeshipProgramme(GetTrainingProgrammesResponseItem source)
    {
        return new ApprenticeshipProgramme
        {
            Id = source.Id,
            ApprenticeshipType = source.ApprenticeshipType,
            Title = source.Title,
            EffectiveFrom = source.EffectiveFrom,
            EffectiveTo = source.EffectiveTo,
            LastDateStarts = source.LastDateStarts,
            ApprenticeshipLevel = source.ApprenticeshipLevel,
            Duration = source.Duration,
            IsActive = source.IsActive,
            EducationLevelNumber = source.EducationLevelNumber
        };
    }
}