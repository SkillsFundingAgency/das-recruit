using System.Linq;
using Esfa.Recruit.Vacancies.Jobs.TrainingTypes.Models;
using SFA.DAS.Apprenticeships.Api.Types;

namespace System.Collections.Generic
{
    public static class ApprenticeshipApiCollectionExtensions
    {
        public static IEnumerable<ApprenticeshipProgramme> FilterAndMapToApprenticeshipProgrammes(this IEnumerable<StandardSummary> standards)
        {
            var list = standards.ToList();

            if (list == null || list.Count == 0)
                return new List<ApprenticeshipProgramme>(0);

            return list.Where(IsStandardActive()).Select(x => new ApprenticeshipProgramme
            {
                Id = x.Id,
                ApprenticeshipType = ApprenticeshipType.Standard,
                Title = x.Title,
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                Level = x.Level,
                Duration = x.Duration
            });
        }

        public static IEnumerable<ApprenticeshipProgramme> FilterAndMapToApprenticeshipProgrammes(this IEnumerable<FrameworkSummary> frameworks)
        {
            var list = frameworks.ToList();

            if (list == null || list.Count == 0)
                return new List<ApprenticeshipProgramme>(0);

            return list.Where(IsFrameworkActive()).Select(x => new ApprenticeshipProgramme
            {
                Id  = x.Id,
                ApprenticeshipType = ApprenticeshipType.Framework,
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
                && (!x.EffectiveTo.HasValue || (x.EffectiveTo.HasValue && x.EffectiveTo.Value.Date >= DateTime.UtcNow.Date ));
        }

        private static Func<FrameworkSummary, bool> IsFrameworkActive()
        {
            return x => x.EffectiveFrom.HasValue && x.EffectiveFrom.Value.Date <= DateTime.UtcNow.Date
                && (!x.EffectiveTo.HasValue || (x.EffectiveTo.HasValue && x.EffectiveTo.Value.Date >= DateTime.UtcNow.Date ));
        }
    }
}
