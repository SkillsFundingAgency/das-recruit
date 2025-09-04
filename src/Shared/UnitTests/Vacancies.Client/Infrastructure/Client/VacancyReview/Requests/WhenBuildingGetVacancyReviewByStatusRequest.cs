using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.VacancyReview.Requests;

public class WhenBuildingGetVacancyReviewByStatusRequest
{
    [Test, AutoData]
    public void Then_The_Url_Is_Constructed_Correctly_With_ReviewStatus(List<ReviewStatus> status)
    {
        var actual = new GetVacancyReviewByFilterRequest(status);
        
        actual.GetUrl.Should().Be($"VacancyReviews?reviewStatus={string.Join("&reviewStatus=",status)}&expiredAssignationDateTime=");
    }
    
    [Test, AutoData]
    public void Then_The_Url_Is_Constructed_Correctly_With_ExpiredAssignationDateTime(DateTime expiredAssignationDateTime)
    {
        var actual = new GetVacancyReviewByFilterRequest(expiredAssignationDateTime:expiredAssignationDateTime);
        
        actual.GetUrl.Should().Be($"VacancyReviews?reviewStatus=&expiredAssignationDateTime={expiredAssignationDateTime}");
    }
}