using System;

namespace Esfa.Recruit.Shared.Web.TagHelpers;

public enum HeadingSize
{
    ExtraLarge,
    Large,
    Medium,
    Small,
    Custom,
}

public static class HeadingSizeExtensions
{
    public static string? GetCssClass(this HeadingSize headingSize)
    {
        return headingSize switch
        {
            HeadingSize.ExtraLarge => "govuk-heading-xl",
            HeadingSize.Large => "govuk-heading-l",
            HeadingSize.Medium => "govuk-heading-m",
            HeadingSize.Small => "govuk-heading-s",
            HeadingSize.Custom => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}