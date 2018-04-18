using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Services.Models
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
    }
}