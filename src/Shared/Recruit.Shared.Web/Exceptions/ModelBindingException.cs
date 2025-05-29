using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Esfa.Recruit.Shared.Web.Exceptions;

public class ModelBindingException(ModelStateDictionary modelState) : Exception(CreateMessage(modelState))
{
    private static string CreateMessage(ModelStateDictionary modelState)
    {
        if (modelState == null)
        {
            return null;
        }

        var failures = modelState
            .Where(entry => entry.Value.Errors is { Count: > 0 })
            .Select(entry => $"'{entry.Key}' because {string.Join(", ", entry.Value.Errors.Select(x => $"[{x.ErrorMessage}]"))}")
            .ToList();
        
        return failures is { Count: >0 }
            ? $"Model binding failed for:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}"
            : "ModelState was invalid but no errors were found.";
    }
}