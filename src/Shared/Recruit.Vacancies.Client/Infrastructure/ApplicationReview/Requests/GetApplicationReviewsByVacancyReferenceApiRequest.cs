﻿using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests
{
    public record GetApplicationReviewsByVacancyReferenceApiRequest(long VacancyReference) : IGetApiRequest
    {
        public string GetUrl
        {
            get
            {
                return $"applicationReviews/vacancyReference/{VacancyReference}";
            }
        }
    }
}