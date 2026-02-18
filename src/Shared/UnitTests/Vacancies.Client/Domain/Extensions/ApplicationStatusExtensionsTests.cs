using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Domain.Extensions;

public class ApplicationStatusExtensionsTests
{
    [TestCase(ApplicationReviewStatus.New, "govuk-tag govuk-tag--light-blue", "govuk-tag govuk-tag--light-blue")]
    [TestCase(ApplicationReviewStatus.Successful, "govuk-tag govuk-tag--green", "govuk-tag govuk-tag--green")]
    [TestCase(ApplicationReviewStatus.Unsuccessful, "govuk-tag govuk-tag--orange", "govuk-tag govuk-tag--orange")]
    [TestCase(ApplicationReviewStatus.Shared, "govuk-tag govuk-tag--yellow", "govuk-tag govuk-tag--yellow")]
    [TestCase(ApplicationReviewStatus.InReview, "govuk-tag govuk-tag--yellow", "govuk-tag govuk-tag--yellow")]
    [TestCase(ApplicationReviewStatus.Interviewing, "govuk-tag govuk-tag--purple", "govuk-tag govuk-tag--purple")]
    [TestCase(ApplicationReviewStatus.EmployerInterviewing, "govuk-tag govuk-tag--pink", "govuk-tag govuk-tag--pink")]
    [TestCase(ApplicationReviewStatus.EmployerUnsuccessful, "govuk-tag govuk-tag--orange", "govuk-tag govuk-tag--orange")]
    public void Then_Provider_And_Employer_Mappings_Are_Canonical(ApplicationReviewStatus status, string expectedProviderClass, string expectedEmployerClass)
    {
        var providerClass = status.GetCssClassForApplicationReviewStatus();
        var employerClass = status.GetCssClassForApplicationReviewStatusForEmployer();

        providerClass.Should().Be(expectedProviderClass);
        employerClass.Should().Be(expectedEmployerClass);
    }
}