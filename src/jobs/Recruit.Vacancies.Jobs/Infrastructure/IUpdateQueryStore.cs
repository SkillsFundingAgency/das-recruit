using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Jobs.TrainingTypes.Models;

namespace Esfa.Recruit.Vacancies.Jobs.Infrastructure
{
    public interface IUpdateQueryStore
    {
        Task UpdateStandardsAndFrameworksAsyc(ApprenticeshipProgrammeView updatedList);
    }
}