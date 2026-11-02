using System;
using System.Text.RegularExpressions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using FluentValidation;
using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators;

internal class HtmlValidator<T, TProperty> : PropertyValidator<T, TProperty>
{
    public override string Name => "HtmlValidator";

    public const string HtmlRegularExpression = @"^[a-zA-Z0-9\u0080-\uFFA7?$@#()""'!,+\-=_:;.&€£*%\s\/<>\[\]]+$";
    private IHtmlSanitizerService _sanitizer;
    private Regex _regex;

    public HtmlValidator(IHtmlSanitizerService sanitizer)
    {
        _regex = CreateRegEx();
        _sanitizer = sanitizer;
    }
    protected override string GetDefaultMessageTemplate(string errorCode) 
    {
        return base.GetDefaultMessageTemplate("{PropertyName} must contain valid characters");
    }

        
    public override bool IsValid(ValidationContext<T> context, TProperty PropertyValue)
    {
        var value = PropertyValue as string;

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