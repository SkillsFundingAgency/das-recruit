using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData
{
    public interface IReferenceDataReader
    {
        Task<T> GetReferenceData<T>() where T : class, IReferenceDataItem;
    }
}
