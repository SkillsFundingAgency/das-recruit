using System;
using System.Text.RegularExpressions;
using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators
{
    internal class FreeTextValidator : PropertyValidator, IRegularExpressionValidator 
    {
		private readonly Regex _regex;

		private const string ValidCharactersExpression = @"^[a-zA-Z0-9\u0080-\uFFA7?$@#()""'!,+\-=_:;.&€£*%\s\/\[\]]*$";

		internal FreeTextValidator() : base("{PropertyName} must contain valid characters")
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

		public string Expression => ValidCharactersExpression;

		private static Regex CreateRegEx()
		{
			try
			{
				if (AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null) {
					return new Regex(ValidCharactersExpression, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2.0));
				}
			}
			catch (Exception)
			{
			}

            return new Regex(ValidCharactersExpression, RegexOptions.IgnoreCase);
		}
	}
}
