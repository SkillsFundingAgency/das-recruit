using System;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators;

internal class PostCodeValidator<T, TProperty> : PropertyValidator<T, TProperty>, IRegularExpressionValidator 
{
    public override string Name => "PostCodeValidator";

    private readonly Regex _regex;

    public PostCodeValidator()
    {
        _regex = CreateRegEx();
    }
        
    protected override string GetDefaultMessageTemplate(string errorcode)
    {
        return base.GetDefaultMessageTemplate("{PropertyName} must be a valid postcode format");
    }


    public override bool IsValid(ValidationContext<T> context, TProperty PropertyValue) {
        if (PropertyValue == null) return true;

        if (!_regex.IsMatch(PropertyValue as string)) {
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