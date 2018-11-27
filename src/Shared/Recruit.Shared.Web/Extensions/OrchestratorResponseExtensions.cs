using Esfa.Recruit.Shared.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class OrchestratorResponseExtensions
    {
        public static void AddErrorsToModelState(this OrchestratorResponse response, ModelStateDictionary modelState)
        {
            if (!response.Success && response.Errors?.Errors !=  null)
            {
                foreach (var error in response.Errors.Errors)
                {
                    modelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            }
        }
    }
}