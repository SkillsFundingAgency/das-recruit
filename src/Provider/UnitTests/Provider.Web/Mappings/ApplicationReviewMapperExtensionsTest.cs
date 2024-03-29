﻿using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Mappings.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Mappings;

public class ApplicationReviewMapperExtensionsTest
{
    [Test, AutoData]
    public void When_Mapping_To_View_Model(
        ApplicationReview applicationReview)
    {
        var source = applicationReview;
        var actual = source.ToViewModel();

        actual.Email.Should().Be(source.Application.Email);
        actual.AdditionalQuestion1.Should().Be(source.AdditionalQuestion1);
        actual.AdditionalQuestion2.Should().Be(source.AdditionalQuestion2);
        actual.AdditionalQuestionAnswer1.Should().Be(source.Application.AdditionalQuestion1);
        actual.AdditionalQuestionAnswer2.Should().Be(source.Application.AdditionalQuestion2);
    }

}