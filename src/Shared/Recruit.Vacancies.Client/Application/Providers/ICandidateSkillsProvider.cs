using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Providers
{
    public interface ICandidateSkillsProvider
    {
        Task<List<string>> GetCandidateSkillsAsync();
    }
}
