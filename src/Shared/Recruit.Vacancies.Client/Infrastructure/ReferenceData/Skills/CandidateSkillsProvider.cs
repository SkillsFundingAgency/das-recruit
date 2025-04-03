using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Skills;

public class CandidateSkillsProvider : ICandidateSkillsProvider
{
    public Task<List<string>> GetCandidateSkillsAsync()
    {
        return Task.FromResult<List<string>>(
        [
            "Communication skills", 
            "IT skills", 
            "Attention to detail", 
            "Organisation skills", 
            "Customer care skills", 
            "Problem solving skills", 
            "Presentation skills", 
            "Administrative skills", 
            "Number skills", 
            "Analytical skills", 
            "Logical", 
            "Team working", 
            "Creative", 
            "Initiative", 
            "Non judgemental", 
            "Patience", 
            "Physical fitness"
        ]);
    }
}