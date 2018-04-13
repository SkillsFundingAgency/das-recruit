using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using SFA.DAS.Apprenticeships.Api.Types;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Models;

namespace System.Collections.Generic
{
    public static class ApprenticeshipApiCollectionExtensions
    {
        public static IEnumerable<ApprenticeshipProgramme> FilterAndMapToApprenticeshipProgrammes(this IEnumerable<StandardSummary> standards, ITimeProvider timeProvider)
        {
            if (standards == null || standards.Count() == 0)
                return Enumerable.Empty<ApprenticeshipProgramme>();

            return standards.Select(x => new ApprenticeshipProgramme
            {
                Id = x.Id,
                ApprenticeshipType = TrainingType.Standard,
                Title = x.Title,
                IsActive = IsStandardActive(timeProvider, x),
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                Level = (ProgrammeLevel)x.Level,
                Duration = x.Duration
            });
        }

        public static IEnumerable<ApprenticeshipProgramme> FilterAndMapToApprenticeshipProgrammes(this IEnumerable<FrameworkSummary> frameworks, ITimeProvider timeProvider)
        {
            if (frameworks == null || frameworks.Count() == 0)
                return Enumerable.Empty<ApprenticeshipProgramme>();

            return frameworks.Select(x => new ApprenticeshipProgramme
            {
                Id  = x.Id,
                ApprenticeshipType = TrainingType.Framework,
                Title = x.Title,
                IsActive = IsFrameworkActive(timeProvider, x),
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                Level = (ProgrammeLevel)x.Level,
                Duration = x.Duration
            });
        }

        private static bool IsStandardActive(ITimeProvider timeProvider, StandardSummary standard)
        {
            return standard.EffectiveFrom.HasValue && standard.EffectiveFrom.Value.Date <= DateTime.UtcNow.Date
                && (!standard.EffectiveTo.HasValue || standard.EffectiveTo.Value.Date >= DateTime.UtcNow.Date);
        }

        private static bool IsFrameworkActive(ITimeProvider timeProvider, FrameworkSummary framework)
        {
            return framework.EffectiveFrom.HasValue && framework.EffectiveFrom.Value.Date <= DateTime.UtcNow.Date
                && (!framework.EffectiveTo.HasValue || framework.EffectiveTo.Value.Date >= DateTime.UtcNow.Date);
        }
    }
}
