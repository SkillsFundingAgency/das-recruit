using System;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.Extensions
{
    public static class EmployerNameOptionViewModelExtensions
    {
        public static EmployerNameOption ConvertToDomainOption(this EmployerNameOptionViewModel model)
        {
            return model == EmployerNameOptionViewModel.RegisteredName 
                ? EmployerNameOption.RegisteredName : EmployerNameOption.TradingName;
        }

        public static EmployerNameOptionViewModel ConvertToModelOption(this EmployerNameOption option)
        {
            switch(option)
            {
                case EmployerNameOption.RegisteredName:
                    return EmployerNameOptionViewModel.RegisteredName;
                case EmployerNameOption.TradingName:
                    return EmployerNameOptionViewModel.ExistingTradingName;
                default: 
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}