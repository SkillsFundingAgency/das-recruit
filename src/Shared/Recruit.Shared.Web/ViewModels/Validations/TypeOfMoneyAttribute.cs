namespace Esfa.Recruit.Shared.Web.ViewModels.Validations
{
    using Esfa.Recruit.Shared.Web.Extensions;    
    using System.ComponentModel.DataAnnotations;

    public class TypeOfMoneyAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            return (((string)value).AsMoney() != null);            
        }
    }
}
