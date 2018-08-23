using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Mappings.Extensions
{
    public static class QualificationsMapperExtensions
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
    }
}
