using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Esfa.Recruit.Shared.Web;

public static class ModelStateDictionaryExtensions
{
    public static void AddValidationErrorsWithFieldMappings(this ModelStateDictionary modelState, EntityValidationResult validationResult, Dictionary<string, string> fieldMappings)
    {
        var validationMappings = fieldMappings?.Select(x => KeyValuePair.Create(x.Key, Tuple.Create<string, string>(x.Value, null))).ToDictionary(x => x.Key, x => x.Value);
        AddValidationErrorsWithMappings(modelState, validationResult, validationMappings);
    }
    
    public static void AddValidationErrorsWithMappings(this ModelStateDictionary modelState, EntityValidationResult validationResult, Dictionary<string, Tuple<string, string>> validationMappings)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentNullException.ThrowIfNull(validationResult);

        foreach (var error in validationResult.Errors)
        {
            Tuple<string, string> mapping = null; 
            if (!validationMappings?.TryGetValue(error.PropertyName, out mapping) ?? false)
            {
                validationMappings.TryGetValue(error.ErrorCode, out mapping);
            }
            modelState.AddModelError(mapping?.Item1 ?? error.PropertyName, mapping?.Item2 ?? error.ErrorMessage);
        }
    }
}