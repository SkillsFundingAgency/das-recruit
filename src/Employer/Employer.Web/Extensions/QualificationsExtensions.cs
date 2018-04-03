using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class QualificationsExtensions
    {
        public static List<QualificationEditModel> ToViewModel(this List<Qualification> qualifications)
        {
            if (qualifications == null)
            {
                return new List<QualificationEditModel>();
            }

            return qualifications.Select(q => new QualificationEditModel
            {
                QualificationType = q.QualificationType,
                Subject = q.Subject,
                Grade = q.Grade,
                Weighting = q.Weighting
            }).ToList();

        }
        
        public static List<Qualification> ToEntity(this List<QualificationEditModel> qualifications)
        {
            if (qualifications == null)
            {
                return new List<Qualification>();
            }
            
            return qualifications.Select(q => new Qualification
            {
                QualificationType = q.QualificationType,
                Subject = q.Subject,
                Grade = q.Grade.ToUpper(),
                Weighting = q.Weighting
            }).ToList();
        }

        public static List<string> AsText(this List<Qualification> qualifications)
        {
            if (qualifications == null)
            {
                return new List<string>();
            }

            return qualifications.Select(q =>
                $"{q.QualificationType} {q.Subject} (Grade {q.Grade}) {q.Weighting.GetDisplayName().ToLower()}").ToList();
        }
    }
}
