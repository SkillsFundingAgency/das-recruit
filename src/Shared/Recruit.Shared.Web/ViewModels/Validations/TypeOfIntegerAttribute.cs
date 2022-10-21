namespace Esfa.Recruit.Shared.Web.ViewModels.Validations
{
    using System.ComponentModel.DataAnnotations;

    public class TypeOfIntegerAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            return (int.TryParse((string)value, out var i));
        }
    }
}