using System;
using System.Text.RegularExpressions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators
{
    internal class HtmlValidator : PropertyValidator
    {
        public const string HtmlRegularExpression = @"^[a-zA-Z0-9\u0080-\uFFA7?$@#()""'!,+\-=_:;.&€£*%\s\/<>\[\]]+$";
        private readonly IHtmlSanitizerService _sanitizer;
        private readonly Regex _regex;

        internal HtmlValidator(IHtmlSanitizerService sanitizer) : base("{PropertyName} must contain allowed HTML")
        {
            _regex = CreateRegEx();
            _sanitizer = sanitizer;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = (string) context.PropertyValue;

            var isHtmlValid = _sanitizer.IsValid(value);

            if (isHtmlValid == false)
                return false;

            if (string.IsNullOrWhiteSpace(value) == false && 
                _regex.IsMatch(value) == false)
            {
                return false;
            }

            return true;
        }

        private static Regex CreateRegEx()
        {
            try
            {
                if (AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null)
                {
                    return new Regex(HtmlRegularExpression, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2.0));
                }
            }
            catch (Exception)
            {
            }

            return new Regex(HtmlRegularExpression, RegexOptions.IgnoreCase);
        }
    }
}
