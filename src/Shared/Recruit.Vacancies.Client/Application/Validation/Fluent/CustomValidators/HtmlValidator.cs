using System;
using System.Text.RegularExpressions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using FluentValidation;
using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators;

internal class HtmlValidator<T, TProperty>(IHtmlSanitizerService sanitizer) : PropertyValidator<T, TProperty>
{
    public override string Name => "HtmlValidator";

    private const string HtmlRegularExpression = @"^[a-zA-Z0-9\u0080-\uFFA7?$@#()""'!,+\-=_:;.&€£*%\s\/<>\[\]]+$";
    private readonly Regex _regex = CreateRegEx();

    protected override string GetDefaultMessageTemplate(string errorCode) 
    {
        return base.GetDefaultMessageTemplate("{PropertyName} must contain valid characters");
    }

        
    public override bool IsValid(ValidationContext<T> context, TProperty PropertyValue)
    {
        var value = PropertyValue as string;

        var isHtmlValid = sanitizer.IsValid(value);

        if (!isHtmlValid)
            return false;

        return string.IsNullOrWhiteSpace(value) || 
               _regex.IsMatch(value);
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
            // ignore any exception and fallback to default constructor
        }

        return new Regex(HtmlRegularExpression, RegexOptions.IgnoreCase);
    }
}