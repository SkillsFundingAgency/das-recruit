using System;

namespace Esfa.Recruit.Qa.Web.Exceptions
{
    public class UnassignedVacancyReviewException : Exception
    {
        public UnassignedVacancyReviewException(string message) : base(message) { }
    }
}
