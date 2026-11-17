using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services;

public interface ISlaService
{
    Task<DateTime> GetSlaDeadlineAsync(DateTime date);
}