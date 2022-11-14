using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    public sealed class EntityValidator<T, TRules> : IEntityValidator<T, TRules> where TRules : struct, IComparable, IConvertible, IFormattable 
    {
        private readonly AbstractValidator<T> _validator;
    
        public EntityValidator(AbstractValidator<T> fluentValidator)
        {
            _validator = fluentValidator;
        }

        public void ValidateAndThrow(T entity, TRules rules)
        {
            var validationResult = ValidateEntity(entity, rules).Result;

            if (validationResult.HasErrors)
            {
                throw new EntityValidationException($"Entity: {typeof(T)} has failed validation", validationResult);
            }
        }

        public EntityValidationResult Validate(T entity, TRules rules)
        {
            return ValidateEntity(entity, rules).Result;
        }

        private async Task<EntityValidationResult> ValidateEntity(T entity, TRules rules)
        {
            var context = new ValidationContext<T>(entity);

            context.RootContextData.Add(ValidationConstants.ValidationsRulesKey, rules);

            var fluentResult = await _validator.ValidateAsync(context);

            if (!fluentResult.IsValid)
            {
                return CreateValidationErrors(fluentResult);
            }

            return new EntityValidationResult();
        }

        private EntityValidationResult CreateValidationErrors(ValidationResult fluentResult)
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