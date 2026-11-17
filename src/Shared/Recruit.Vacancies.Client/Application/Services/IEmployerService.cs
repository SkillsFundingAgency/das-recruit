using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Services;

public interface IEmployerService
{
    Task<string> GetEmployerNameAsync(Vacancy vacancy);
    Task<string> GetEmployerDescriptionAsync(Vacancy vacancy);
}