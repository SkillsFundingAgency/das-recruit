using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public class QualificationsProvider : IQualificationsProvider
    {
        private readonly IReferenceDataReader _referenceDataReader;

        public QualificationsProvider(IReferenceDataReader referenceDataReader)
        {
            _referenceDataReader = referenceDataReader;
        }

        public async Task<IList<string>> GetQualificationsAsync()
        {
            var data = await _referenceDataReader.GetReferenceData<Qualifications>();

            return data.QualificationTypes;
        }
    }
}