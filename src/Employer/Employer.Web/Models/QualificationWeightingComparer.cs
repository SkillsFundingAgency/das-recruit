using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.Models
{
    public class QualificationWeightingComparer : Comparer<QualificationWeighting?>
    {
        public override int Compare(QualificationWeighting? x, QualificationWeighting? y)
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
        }
    }
}
