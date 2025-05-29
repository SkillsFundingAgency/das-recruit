using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Shared.Web.UnitTests.Extensions;

public class QualificationExtensions
{
    [Test, AutoData]
    public void Then_Other_Qualifications_For_FaaV2_Are_Mapped(string grade, int level, string subject, string otherQualificationName)
    {
        var qualification = new Qualification
        {
            Grade = grade,
            Level = level,
            Subject = subject,
            QualificationType = "Other",
            OtherQualificationName = otherQualificationName
        };

        var actual = new List<Qualification> { qualification }.AsText();

        actual.First().Should()
            .Be(
                $"{qualification.QualificationType} (Level {qualification.Level}) ({qualification.OtherQualificationName}) {qualification.Subject} (Grade {qualification.Grade}) {qualification.Weighting.GetDisplayName().ToLower()}");
    }
    
    [Test, AutoData]
    public void Then_Qualifications_For_FaaV2_Are_Mapped(string grade, string subject)
    {
        var qualification = new Qualification
        {
            Grade = grade,
            Subject = subject,
            QualificationType = "A Level"
        };

        var actual = new List<Qualification> { qualification }.AsText();

        actual.First().Should()
            .Be(
                $"{qualification.QualificationType} {qualification.Subject} (Grade {qualification.Grade}) {qualification.Weighting.GetDisplayName().ToLower()}");
    }
}