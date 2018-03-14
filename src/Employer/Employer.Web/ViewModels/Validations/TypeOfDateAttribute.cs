using System.Globalization;
using Esfa.Recruit.Employer.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class TypeOfDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return ((string) value).AsDateTimeUk() != null;
        }
    }
}
