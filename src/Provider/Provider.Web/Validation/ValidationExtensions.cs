using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using FluentValidation.Results;

namespace Esfa.Recruit.Provider.Web.Validation;

public static class ValidationExtensions
{
    private static long ConvertCustomState(object customState)
    {
        return customState switch
        {
            long value => value,
            _ => -1
        };
    }
    
    private static List<EntityValidationError> ToEntityValidationErrors(this List<ValidationFailure> validationErrors)
    {
        return validationErrors
            .Select(error => new EntityValidationError(
                ConvertCustomState(error.CustomState),
                error.PropertyName,
                error.ErrorMessage,
                error.ErrorCode)
            )
            .ToList();
    }

    public static void AddValidationErrors(this IList<EntityValidationError> errors, List<ValidationFailure> validationErrors)
    {
        validationErrors
            .ToEntityValidationErrors()
            .ForEach(errors.Add);
    }
}