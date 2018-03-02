using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Jobs.Models;

namespace Esfa.Recruit.Vacancies.Jobs
{
    public interface IUpdateQueryStore
    {
        Task UpdateStandardsAndFrameworksAsyc(ApprenticeshipProgrammeView updatedList);
    }
}