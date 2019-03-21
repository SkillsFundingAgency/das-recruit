using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IEmployerNameService
    {
         Task<string> GetEmployerName(Guid vacancyId);
    }
}