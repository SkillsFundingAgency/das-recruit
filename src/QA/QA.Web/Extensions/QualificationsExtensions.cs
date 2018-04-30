using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Qa.Web.Extensions
{
    //NOTE: Duplicated from Employer solution
    public static class QualificationsExtensions
    {
        
        public static IOrderedEnumerable<Qualification> SortQualifications(this IEnumerable<Qualification> qualifications, IList<string> qualificationTypes)
        {
            return qualifications?.OrderBy(q => qualificationTypes.IndexOf(q.QualificationType))
                .ThenBy(q => q.Weighting, WeightingComparer)
                .ThenBy(q => q.Subject);
        }

        private static readonly Comparer<QualificationWeighting?> WeightingComparer = Comparer<QualificationWeighting?>.Create((x, y) =>
        {
            if (x == y)
            {
                return 0;
            }

            if (x == QualificationWeighting.Essential)
            {
                return -1;
            }

            return 1;
        });

        public static IEnumerable<string> AsText(this IEnumerable<Qualification> qualifications)
        {
            if (qualifications == null)
            {
                return Enumerable.Empty<string>();
            }
            
            return qualifications.Select(q => $"{q.QualificationType} {q.Subject} (Grade {q.Grade}) {q.Weighting.GetDisplayName().ToLower()}");
        }
    }
}