namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    using Esfa.Recruit.Employer.Web.Extensions;    
    using System.ComponentModel.DataAnnotations;

    public class TypeOfMoneyGBPAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return (((string)value).AsMoney() != null);            
        }
    }
}
