using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IVacancyTransferService
    {
        Task TransferVacancyToLegalEntityAsync(Vacancy vacancy, VacancyUser initiatingUser, TransferReason transferReason);
    }
}