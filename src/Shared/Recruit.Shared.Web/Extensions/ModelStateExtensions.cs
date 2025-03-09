using Esfa.Recruit.Shared.Web.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Esfa.Recruit.Shared.Web.Extensions;

public static class ModelStateExtensions
{
    public static void ThrowIfBindingErrors(this ModelStateDictionary modelState)
    {
        if (modelState == null || modelState.IsValid)
        {
            return;
        }

        throw new ModelBindingException(modelState);
    }
}