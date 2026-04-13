using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Shared.Web.Orchestrators
{
    public class OrchestratorResponse(bool isSuccessful)
    {
        public OrchestratorResponse(EntityValidationResult errorResult) : this(false) => Errors = errorResult;

        public bool Success { get; set; } = isSuccessful;
        public EntityValidationResult Errors { get; set; } = new();
    }
}