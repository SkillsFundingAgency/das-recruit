using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.Services
{
    public interface ISlaService
    {
        Task<DateTime> GetSlaDeadlineAsync(DateTime date);
    }
}
