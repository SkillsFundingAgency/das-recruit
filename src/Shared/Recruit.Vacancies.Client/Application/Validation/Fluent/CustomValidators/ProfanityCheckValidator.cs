using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Extensions;
using FluentValidation;
using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators;

public class ProfanityCheckValidator<T, TProperty> : PropertyValidator<T, TProperty>
{
    public override string Name => "ProfanityCheckValidator";

    private readonly IProfanityListProvider _profanityListProvider;

    public ProfanityCheckValidator(IProfanityListProvider profanityListProvider)
    {
        _profanityListProvider = profanityListProvider;
    }
        
    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return base.GetDefaultMessageTemplate("{PropertyName} must not contain a banned word or phrase.");
    }


    public override bool IsValid(ValidationContext<T> context, TProperty PropertyValue)
    {
        var profanityList = _profanityListProvider.GetProfanityListAsync();

        var freeText = PropertyValue as string;

        var formatForParsing = freeText.FormatForParsing();

        foreach (var profanity in profanityList.Result)
        {
            if (freeText != null)
            {
                var occurrences = formatForParsing.CountOccurrences(profanity);

                if (occurrences > 0)
                {
                    return false;
                }
            }
        }
        return true;
    }
}