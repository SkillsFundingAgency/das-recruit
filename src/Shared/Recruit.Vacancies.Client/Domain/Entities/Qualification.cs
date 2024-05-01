using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Qualification : IEquatable<Qualification>

    {
        public string QualificationType { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public int? Level { get; set; }
        public QualificationWeighting? Weighting { get; set; }
        public string OtherQualificationName { get; set; }

        public bool Equals(Qualification other)
        {
            if (other == null)
                return false;

            return (QualificationType == null || QualificationType.Equals(other.QualificationType)) &&
                    (Subject == null || Subject.Equals(other.Subject)) &&
                    (Grade == null || Grade.Equals(other.Grade)) &&
                    (Weighting == null || Weighting.Equals(other.Weighting)) && 
                    (Level == null || Level.Equals(other.Level)) &&
                    (OtherQualificationName == null || OtherQualificationName.Equals(other.OtherQualificationName));
        }
    }
}
