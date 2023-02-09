using Esfa.Recruit.Shared.Web.Extensions;

namespace Esfa.Recruit.Shared.Web.ViewModels.Validations
{
    using System.ComponentModel.DataAnnotations;

    public class TypeOfDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            return ((string) value).AsDateTimeUk() != null;
        }
    }
}
