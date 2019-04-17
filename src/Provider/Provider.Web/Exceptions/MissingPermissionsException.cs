using Esfa.Recruit.Provider.Web.Model;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Provider.Web.Exceptions
{
    public class MissingPermissionsException : RecruitException
    {
        public RecruitVacancyAction VacancyAction { get; private set; }

        public MissingPermissionsException(string message, RecruitVacancyAction va) : base(message)
        {
            VacancyAction = va;
        }
    }
}