using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IEmployerNameService
    {
         Task<string> GetEmployerNameAsync(Guid vacancyId);
    }
}