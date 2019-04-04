using System;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class EmployerNameOptionViewModelExtensions
    {
        public static EmployerNameOption ConvertToDomainOption(this EmployerNameOptionViewModel model)
        {
            switch (model)
            {
                case EmployerNameOptionViewModel.RegisteredName:
                    return EmployerNameOption.RegisteredName;
                case EmployerNameOptionViewModel.ExistingTradingName:
                    return EmployerNameOption.TradingName;
                case EmployerNameOptionViewModel.NewTradingName:
                    return EmployerNameOption.TradingName;
                case EmployerNameOptionViewModel.Anonymous:
                    return EmployerNameOption.Anonymous;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static EmployerNameOptionViewModel ConvertToModelOption(this EmployerNameOption option)
        {
            switch(option)
            {
                case EmployerNameOption.RegisteredName:
                    return EmployerNameOptionViewModel.RegisteredName;
                case EmployerNameOption.TradingName:
                    return EmployerNameOptionViewModel.ExistingTradingName;
                case EmployerNameOption.Anonymous:
                    return EmployerNameOptionViewModel.Anonymous;
                default: 
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}