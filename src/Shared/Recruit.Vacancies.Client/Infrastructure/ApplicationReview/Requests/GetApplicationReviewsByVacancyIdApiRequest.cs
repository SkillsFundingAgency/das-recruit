using System;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;

public record GetApplicationReviewsByVacancyIdApiRequest(Guid VacancyId) : IGetApiRequest
{
    public string GetUrl => $"applicationReviews/vacancyId/{VacancyId}";
}