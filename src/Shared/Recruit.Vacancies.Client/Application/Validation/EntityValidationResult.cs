using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation;

public class EntityValidationResult
{
    public bool HasErrors => Errors?.Count() > 0;

    public IList<EntityValidationError> Errors { get; set; } = new List<EntityValidationError>();

    public static EntityValidationResult FromFluentValidationResult(ValidationResult fluentResult)
    {
        var result = new EntityValidationResult();

        if (fluentResult.IsValid == false && fluentResult.Errors.Count > 0)
        {
            foreach (var fluentError in fluentResult.Errors)
            {
                result.Errors.Add(new EntityValidationError(long.Parse(fluentError.ErrorCode), fluentError.PropertyName, fluentError.ErrorMessage, fluentError.ErrorCode));
            }
        }

        return result;
    }
}