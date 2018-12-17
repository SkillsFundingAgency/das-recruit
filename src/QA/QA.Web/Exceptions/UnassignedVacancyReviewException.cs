using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Qa.Web.Exceptions
{
    public class UnassignedVacancyReviewException : RecruitException
    {
        public UnassignedVacancyReviewException(string message) : base(message) { }
    }
}
