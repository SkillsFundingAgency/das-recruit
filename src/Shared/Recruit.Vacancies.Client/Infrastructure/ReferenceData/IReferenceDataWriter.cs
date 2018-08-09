using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData
{
    public interface IReferenceDataWriter
    {
        Task UpsertReferenceData<T>(T referenceData) where T : class, IReferenceDataItem;
    }
}
