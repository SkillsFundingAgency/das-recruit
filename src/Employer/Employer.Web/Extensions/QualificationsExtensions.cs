using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class QualificationsExtensions
    {
        public static IEnumerable<QualificationEditModel> ToViewModel(this IEnumerable<Qualification> qualifications)
        {
            if (qualifications == null)
            {
                return Enumerable.Empty<QualificationEditModel>();
            }

            return qualifications.Select(q => new QualificationEditModel
            {
                QualificationType = q.QualificationType,
                Subject = q.Subject,
                Grade = q.Grade,
                Weighting = q.Weighting
            });

        }
        
        public static IEnumerable<Qualification> ToEntity(this IEnumerable<QualificationEditModel> qualifications)
        {
            if (qualifications == null)
            {
                return Enumerable.Empty<Qualification>();
            }
            
            return qualifications.Select(q => new Qualification
            {
                QualificationType = q.QualificationType,
                Subject = q.Subject,
                Grade = q.Grade,
                Weighting = q.Weighting
            });
        }

        public static IEnumerable<string> AsText(this IEnumerable<Qualification> qualifications)
        {
            if (qualifications == null)
            {
                return Enumerable.Empty<string>();
            }
            
            return qualifications.Select(q => $"{q.QualificationType} {q.Subject} (Grade {q.Grade}) {q.Weighting.GetDisplayName().ToLower()}");
        }

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
    }
}
