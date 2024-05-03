using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Qualification : IEquatable<Qualification>

    {
        public string QualificationType { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public QualificationWeighting? Weighting { get; set; }
        public string Level { get; set; }
        
        public bool Equals(Qualification other)
        {
            if (other == null)
                return false;

            return QualificationType.Equals(other.QualificationType) &&
                   Subject.Equals(other.Subject) &&
                   Grade.Equals(other.Grade) &&
                   Weighting.Equals(other.Weighting);
        }
    }
}
