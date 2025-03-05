using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Esfa.Recruit.Shared.Web;

public static class ModelStateDictionaryExtensions
{
    public static void AddValidationErrors(this ModelStateDictionary modelState, EntityValidationResult validationResult, Dictionary<string, string> fieldMappings = null)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentNullException.ThrowIfNull(validationResult);

        foreach (var error in validationResult.Errors)
        {
            string propertyName = fieldMappings?.GetValueOrDefault(error.PropertyName, error.PropertyName) ?? error.PropertyName;
            modelState.AddModelError(propertyName, error.ErrorMessage);
        }
    }
    
    public static void AddValidationErrors(this ModelStateDictionary modelState, EntityValidationResult validationResult, Dictionary<string, Tuple<string, string>> validationMappings = null)
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