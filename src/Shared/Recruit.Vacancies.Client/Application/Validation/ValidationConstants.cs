using System.Text.RegularExpressions;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    public static class ValidationConstants
    {
        internal const string ValidationsRulesKey = "validationsToRun";

        // See http://stackoverflow.com/questions/164979/uk-postcode-regex-comprehensive
        internal const string PostCodeRegExPattern = "(([gG][iI][rR] {0,}0[aA]{2})|((([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y]?[0-9][0-9]?)|(([a-pr-uwyzA-PR-UWYZ][0-9][a-hjkstuwA-HJKSTUW])|([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y][0-9][abehmnprv-yABEHMNPRV-Y]))) {0,}[0-9][abd-hjlnp-uw-zABD-HJLNP-UW-Z]{2}))";

        public static Regex PostcodeRegex => new Regex(PostCodeRegExPattern);
        public static Regex UkprnRegex => new Regex(@"^((?!(0))[0-9]{8})$");
    }
}
