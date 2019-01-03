using System;
using System.Text.RegularExpressions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using FluentValidation.Validators;
using Ganss.XSS;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators
{
    internal class HtmlValidator : PropertyValidator
    {
        public const string HtmlRegularExpression = @"^[a-zA-Z0-9\u0080-\uFFA7?$@#()""'!,+\-=_:;.&€£*%\s\/<>\[\]]+$";
        private readonly IHtmlSanitizerService _sanitizer;
        private readonly Regex _regex;

        internal HtmlValidator(IHtmlSanitizerService sanitizer) : base("{PropertyName} must contain allowed HTML")
        {
            _sanitizer = sanitizer;
            _regex = CreateRegEx();
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
                return true;

            var unsanitized = (string) context.PropertyValue;

            //Property value should have already been through HtmlSanitizerService.Sanitize prior to its setting so sanitized and unsanitized values should be the same.
            var sanitized = _sanitizer.Sanitize(unsanitized);

            if (sanitized.Equals(unsanitized) == false)
                return false;

            if (!_regex.IsMatch(unsanitized))
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
