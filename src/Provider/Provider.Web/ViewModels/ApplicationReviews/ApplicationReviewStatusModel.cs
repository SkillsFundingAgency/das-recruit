using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;

public class ApplicationReviewStatusModel
{
    public string CandidateFeedback { get; set; }
    public Guid VacancyId { get; set; }
}

public class ApplicationReviewsToUpdateStatusModel
{
    public List<Guid> ApplicationReviewIds { get; set; }
    public Guid VacancyId { get; set; }
}