using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class QualificationService : IQualificationsService
    {
        private readonly QualificationsConfiguration _qualificationsConfig;
        private static WeightingComparer _weightingComparer = new WeightingComparer();


        public QualificationService(IOptions<QualificationsConfiguration> qualificationsConfigOptions)
        {
            _qualificationsConfig = qualificationsConfigOptions.Value;
        }

        public List<string> GetQualificationTypes()
        {
            return _qualificationsConfig.QualificationTypes ?? new List<string>();
        }

        public List<Qualification> SortQualifications(IEnumerable<Qualification> qualificationsToSort)
        {
            var qualifications = qualificationsToSort.OrderBy(q => q.Weighting, _weightingComparer)
                .ThenBy(q => q.QualificationType).ThenBy(q => q.Subject).ToList();

            return qualifications;
        }

        private class WeightingComparer : IComparer<QualificationWeighting>
        {
            public int Compare(QualificationWeighting x, QualificationWeighting y)
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
}
