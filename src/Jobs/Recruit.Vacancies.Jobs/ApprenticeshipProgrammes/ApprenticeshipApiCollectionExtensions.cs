using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using SFA.DAS.Apprenticeships.Api.Types;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using Esfa.Recruit.Vacancies.Client.Domain.Services;

namespace System.Collections.Generic
{
    public static class ApprenticeshipApiCollectionExtensions
    {
        public static IEnumerable<ApprenticeshipProgramme> FilterAndMapToApprenticeshipProgrammes(this IEnumerable<StandardSummary> standards, ITimeProvider timeProvider)
        {
            if (standards == null || standards.Count() == 0)
                return Enumerable.Empty<ApprenticeshipProgramme>();

            return standards.Where(IsStandardActive(timeProvider)).Select(x => new ApprenticeshipProgramme
            {
                Id = x.Id,
                ApprenticeshipType = TrainingType.Standard,
                Title = x.Title,
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                Level = x.Level,
                Duration = x.Duration
            });
        }

        public static IEnumerable<ApprenticeshipProgramme> FilterAndMapToApprenticeshipProgrammes(this IEnumerable<FrameworkSummary> frameworks, ITimeProvider timeProvider)
        {
            if (frameworks == null || frameworks.Count() == 0)
                return Enumerable.Empty<ApprenticeshipProgramme>();

            return frameworks.Where(IsFrameworkActive(timeProvider)).Select(x => new ApprenticeshipProgramme
            {
                Id  = x.Id,
                ApprenticeshipType = TrainingType.Framework,
                Title = x.Title,
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                Level = x.Level,
                Duration = x.Duration
            });
        }

        private static Func<StandardSummary, bool> IsStandardActive(ITimeProvider timeProvider)
        {
            return x => x.EffectiveFrom.HasValue && x.EffectiveFrom.Value.Date <= DateTime.UtcNow.Date
                && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= DateTime.UtcNow.Date);
        }

        private static Func<FrameworkSummary, bool> IsFrameworkActive(ITimeProvider timeProvider)
        {
            return x => x.EffectiveFrom.HasValue && x.EffectiveFrom.Value.Date <= DateTime.UtcNow.Date
                && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= DateTime.UtcNow.Date);
        }
    }
}
