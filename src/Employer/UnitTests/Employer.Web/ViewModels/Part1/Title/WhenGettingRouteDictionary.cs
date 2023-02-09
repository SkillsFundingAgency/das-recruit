using System;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.Title;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Part1.Title;

public class WhenGettingRouteDictionary
{
    [Test, AutoData]
    public void Then_Dictionary_Has_Correct_Items(Guid vacancyId, string employerAccountId)
    {
        var actual = new TitleViewModel
        {
            EmployerAccountId = employerAccountId,
            VacancyId = vacancyId
        };

        actual.RouteDictionary.Should().ContainKey("VacancyId");
        actual.RouteDictionary["VacancyId"].Should().Be(vacancyId.ToString());
        actual.RouteDictionary.Should().ContainKey("EmployerAccountId");
        actual.RouteDictionary["EmployerAccountId"].Should().Be(employerAccountId.ToUpper());
    }

    [Test, AutoData]
    public void And_No_VacancyId_Then_Not_Added_To_Dictionary(string employerAccountId)
    {
        var actual = new TitleViewModel
        {
            EmployerAccountId = employerAccountId
        };

        actual.RouteDictionary.Should().NotContainKey("VacancyId");
        actual.RouteDictionary.Should().ContainKey("EmployerAccountId");
        actual.RouteDictionary["EmployerAccountId"].Should().Be(employerAccountId.ToUpper());
    }
}