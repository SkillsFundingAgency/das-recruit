using Esfa.Recruit.Employer.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class EmployerNameOptionViewModelExtensions
    {
        public static EmployerNameOption ConvertToDomainEntity(this EmployerNameOptionViewModel model)
        {
            return model == EmployerNameOptionViewModel.RegisteredName 
                    ? EmployerNameOption.RegisteredName : EmployerNameOption.TradingName;
        }
    }
}