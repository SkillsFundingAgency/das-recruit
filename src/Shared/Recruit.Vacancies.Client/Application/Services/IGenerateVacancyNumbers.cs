using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IGenerateVacancyNumbers
    {
        Task<long> GenerateAsync();
    }
}