using System;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    public class VacancyValidator : IVacancyValidator
    {
        private const string ValidationsToRunKey = "validationsToRun";
        private AbstractValidator<Vacancy> _validator;
    
        public VacancyValidator(AbstractValidator<Vacancy> fluentValidator)
        {
            _validator = fluentValidator;
        }

        public void ValidateAndThrow(Vacancy vacancy, VacancyRuleSet validationsToRun)
        {
            var context = new ValidationContext<Vacancy>(vacancy);

            context.RootContextData.Add(ValidationsToRunKey, validationsToRun);

            var fluentResult = _validator.Validate(context);

            if (!fluentResult.IsValid)
            {
                var validationResult = CreateValidationErrors(fluentResult);

                // TODO: LWA Do we want to add the validations rules that were run??
                throw new EntityValidationException("Vacancy contains validation error", validationResult);
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