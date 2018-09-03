using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IVacancyService
    {
        Task CloseVacancy(Guid vacancyId, Guid commandId);
    }
}