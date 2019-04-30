using System;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class EmployerIdentityOptionExtensions
    {
        public static EmployerNameOption ConvertToDomainOption(this EmployerIdentityOption model)
        {
            switch (model)
            {
                case EmployerIdentityOption.RegisteredName:
                    return EmployerNameOption.RegisteredName;
                case EmployerIdentityOption.ExistingTradingName:
                    return EmployerNameOption.TradingName;
                case EmployerIdentityOption.NewTradingName:
                    return EmployerNameOption.TradingName;
                case EmployerIdentityOption.Anonymous:
                    return EmployerNameOption.Anonymous;
                default:
                    throw new ArgumentException($"Cannot map '{model.ToString()}' to '{nameof(EmployerNameOption)}'");
            }
        }

        public static EmployerIdentityOption ConvertToModelOption(this EmployerNameOption option)
        {
            switch (option)
            {
                case EmployerNameOption.RegisteredName:
                    return EmployerIdentityOption.RegisteredName;
                case EmployerNameOption.TradingName:
                    return EmployerIdentityOption.ExistingTradingName;
                case EmployerNameOption.Anonymous:
                    return EmployerIdentityOption.Anonymous;
                default:
                    throw new ArgumentException($"Cannot map '{option.ToString()}' to '{nameof(EmployerIdentityOption)}'");
            }
        }
    }
}