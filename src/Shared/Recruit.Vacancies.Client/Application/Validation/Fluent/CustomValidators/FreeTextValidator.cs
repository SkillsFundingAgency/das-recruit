using System;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators;

internal class FreeTextValidator <T, TProperty> : PropertyValidator<T, TProperty>, IRegularExpressionValidator 
{
    public override string Name => "FreeTextValidator";

    private Regex _regex;

    private const string ValidCharactersExpression = @"^[a-zA-Z0-9\u0080-\uFFA7?$@#()""'!,+\-=_:;.&€£*%\s\/\[\]]*$";

    protected override string GetDefaultMessageTemplate(string errorCode) 
    {
        return base.GetDefaultMessageTemplate("{PropertyName} must contain valid characters");
    }

    public override bool IsValid(ValidationContext<T> context, TProperty PropertyValue) {
        if (PropertyValue == null) return true;
        _regex = CreateRegEx();
        if (!_regex.IsMatch(PropertyValue as string ?? string.Empty)) {
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