using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData
{
    public interface IReferenceDataWriter
    {
        Task UpsertBankHolidays(BankHolidays bankHolidays);
    }
}
