using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    public class PostcodeAttribute : RegularExpressionAttribute
    {
        // See http://stackoverflow.com/questions/164979/uk-postcode-regex-comprehensive
        private const string RegexPostcode = @"^(([gG][iI][rR] {0,}0[aA]{2})|((([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y]?[0-9][0-9]?)|(([a-pr-uwyzA-PR-UWYZ][0-9][a-hjkstuwA-HJKSTUW])|([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y][0-9][abehmnprv-yABEHMNPRV-Y]))) {0,}[0-9][abd-hjlnp-uw-zABD-HJLNP-UW-Z]{2}))$";

        public PostcodeAttribute() : base(RegexPostcode)
        {
        }
    }
}
