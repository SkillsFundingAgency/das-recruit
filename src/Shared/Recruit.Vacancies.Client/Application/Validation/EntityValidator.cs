using System;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    public class EntityValidator<T, TRules> : IEntityValidator<T, TRules> where TRules : struct, IComparable, IConvertible, IFormattable 
    {
        private const string ValidationsToRunKey = "validationsToRun";
        private AbstractValidator<T> _validator;
    
        public EntityValidator(AbstractValidator<T> fluentValidator)
        {
            _validator = fluentValidator;
        }

        public void ValidateAndThrow(T entity, TRules validationsToRun)
        {
            var context = new ValidationContext<T>(entity);

            context.RootContextData.Add(ValidationsToRunKey, validationsToRun);

            var fluentResult = _validator.Validate(context);

            if (!fluentResult.IsValid)
            {
                var validationResult = CreateValidationErrors(fluentResult);

                // TODO: LWA Do we want to add the validations rules that were run??
                throw new EntityValidationException($"Entity: {typeof(T)} has failed validation", validationResult);
            }
        }

        public EntityValidationResult CreateValidationErrors(ValidationResult fluentResult)
        {
            var newResult = new EntityValidationResult();

            if (fluentResult.IsValid == false && fluentResult.Errors.Count > 0)
            {
                foreach(var fluentError in fluentResult.Errors)
                {
                    newResult.Errors.Add(new EntityValidationError(ParseForRuleId(fluentError.CustomState), fluentError.PropertyName, fluentError.ErrorMessage, fluentError.ErrorCode));
                }
            }

            return newResult;
        }

        private long ParseForRuleId(object customState)
        {
            if (customState == null)
                throw new ArgumentNullException(nameof(customState), "Fluent Error should have CustomState property set to the RuleId");

            try
            {
				return Convert.ToInt64(customState);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Unexpected value for customState. Expecting a long", nameof(customState), ex);
            }
        }
    }
}