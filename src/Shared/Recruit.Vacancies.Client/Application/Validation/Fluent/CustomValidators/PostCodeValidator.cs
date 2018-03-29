using System;
using System.Text.RegularExpressions;
using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators
{
    internal class PostCodeValidator : PropertyValidator, IRegularExpressionValidator 
    {
		private readonly Regex _regex;

        internal PostCodeValidator() : base("{PropertyName} must be a valid postcode format")
        {
			_regex = CreateRegEx();
		}

		protected override bool IsValid(PropertyValidatorContext context) {
			if (context.PropertyValue == null) return true;

			if (!_regex.IsMatch((string)context.PropertyValue)) {
				return false;
			}

			return true;
		}

		public string Expression => ValidationConstants.PostCodeRegExPattern;

		private static Regex CreateRegEx()
		{
			try
			{
				if (AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null) {
					return new Regex(ValidationConstants.PostCodeRegExPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2.0));
				}
			}
            catch
            {
            }

            return new Regex(ValidationConstants.PostCodeRegExPattern, RegexOptions.IgnoreCase);
		}
	}
}
