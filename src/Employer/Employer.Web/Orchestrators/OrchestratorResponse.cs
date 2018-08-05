using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class OrchestratorResponse
    {
        public OrchestratorResponse(bool isSuccessful)
        {
            Success = isSuccessful;
            Errors = new EntityValidationResult();
        }

        public OrchestratorResponse(EntityValidationResult errorResult) : this(false)
        {
            Errors = errorResult;
        }

        public bool Success { get; set; }
        public EntityValidationResult Errors { get; set; }
    }
}