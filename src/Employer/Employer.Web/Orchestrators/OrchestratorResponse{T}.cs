using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class OrchestratorResponse<T> : OrchestratorResponse
    {
        public OrchestratorResponse(T data) : base(true)
        {
            Data = data;
        }

        public OrchestratorResponse(EntityValidationResult errors) : base(false)
        {
            Errors = errors;
        }

        public T Data { get; set; }
    }
}