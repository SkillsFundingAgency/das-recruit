using System;
using Esfa.Recruit.Provider.Web.Model;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.Extensions
{
    public static class EmployerNameOptionViewModelExtensions
    {
        public static EmployerNameOption ConvertToDomainOption(this EmployerIdentityOption model)
        {
            return model == EmployerIdentityOption.RegisteredName 
                ? EmployerNameOption.RegisteredName : EmployerNameOption.TradingName;
        }

        public static EmployerIdentityOption ConvertToModelOption(this EmployerNameOption option)
        {
            switch(option)
            {
                case EmployerNameOption.RegisteredName:
                    return EmployerIdentityOption.RegisteredName;
                case EmployerNameOption.TradingName:
                    return EmployerIdentityOption.ExistingTradingName;
                default:
                    throw new ArgumentException($"Cannot map '{option.ToString()}' to '{nameof(EmployerIdentityOption)}'");
            }
        }
    }
}