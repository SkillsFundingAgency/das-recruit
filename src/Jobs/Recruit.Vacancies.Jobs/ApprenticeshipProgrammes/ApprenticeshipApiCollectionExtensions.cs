using System.Linq;
using SFA.DAS.Apprenticeships.Api.Types;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using Esfa.Recruit.Vacancies.Client.Domain;

namespace System.Collections.Generic
{
    public static class ApprenticeshipApiCollectionExtensions
    {
        public static IEnumerable<ApprenticeshipProgramme> FilterAndMapToApprenticeshipProgrammes(this IEnumerable<StandardSummary> standards)
        {
            if (standards == null || standards.Count() == 0)
                return Enumerable.Empty<ApprenticeshipProgramme>();

            return standards.Where(IsStandardActive()).Select(x => new ApprenticeshipProgramme
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

        public static IEnumerable<ApprenticeshipProgramme> FilterAndMapToApprenticeshipProgrammes(this IEnumerable<FrameworkSummary> frameworks)
        {
            if (frameworks == null || frameworks.Count() == 0)
                return Enumerable.Empty<ApprenticeshipProgramme>();

            return frameworks.Where(IsFrameworkActive()).Select(x => new ApprenticeshipProgramme
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

        private static Func<StandardSummary, bool> IsStandardActive()
        {
            return x => x.EffectiveFrom.HasValue && x.EffectiveFrom.Value.Date <= DateTime.UtcNow.Date
                && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= DateTime.UtcNow.Date);
        }

        private static Func<FrameworkSummary, bool> IsFrameworkActive()
        {
            return x => x.EffectiveFrom.HasValue && x.EffectiveFrom.Value.Date <= DateTime.UtcNow.Date
                && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= DateTime.UtcNow.Date);
        }
    }
}
