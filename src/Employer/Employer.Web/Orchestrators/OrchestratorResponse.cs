using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class OrchestratorResponse
    {
        public static OrchestratorResponse SuccessfulResponse = new OrchestratorResponse(true);

        public OrchestratorResponse(bool isSuccessful)
        {
            Success = isSuccessful;
        }

        public OrchestratorResponse(EntityValidationResult errorResult) : this(false)
        {
            Errors = errorResult;
        }

        public bool Success { get; set; }
        public EntityValidationResult Errors { get; set; }
    }
}