using System;
using System.Text.RegularExpressions;
using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators
{
    public class PostCodeValidator : PropertyValidator, IRegularExpressionValidator 
    {
		private readonly Regex _regex;

        const string expression = @"(([gG][iI][rR] {0,}0[aA]{2})|((([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y]?[0-9][0-9]?)|(([a-pr-uwyzA-PR-UWYZ][0-9][a-hjkstuwA-HJKSTUW])|([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y][0-9][abehmnprv-yABEHMNPRV-Y]))) {0,}[0-9][abd-hjlnp-uw-zABD-HJLNP-UW-Z]{2}))";

        public PostCodeValidator() : base("{PropertyName} must be a valid postcode format")
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

		public string Expression => expression;

		private static Regex CreateRegEx()
		{
			try
			{
				if (AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null) {
					return new Regex(expression, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2.0));
				}
			}
			catch
			{
			}

            return new Regex(expression, RegexOptions.IgnoreCase);
		}
	}
}
