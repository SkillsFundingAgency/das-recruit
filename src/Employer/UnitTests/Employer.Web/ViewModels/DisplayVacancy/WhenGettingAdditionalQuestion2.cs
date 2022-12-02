using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.DisplayVacancy;

public class WhenGettingAdditionalQuestion2
{
    [Test]
    public void And_Null_Then_Returns_Null()
    {
        var vm = new TestDisplayVacancyViewModel();

        vm.AdditionalQuestion2.Should().BeNull();
    }
    
    [Test]
    public void And_Empty_Then_Returns_Null()
    {
        var vm = new TestDisplayVacancyViewModel
        {
            AdditionalQuestion2 = ""
        };

        vm.AdditionalQuestion2.Should().BeNull();
    }
    
    [Test]
    public void And_Whitespace_Then_Returns_Null()
    {
        var vm = new TestDisplayVacancyViewModel
        {
            AdditionalQuestion2 = "  "
        };

        vm.AdditionalQuestion2.Should().BeNull();
    }
    
    [Test, AutoData]
    public void And_Has_Value_Then_Returns_Value(string additionalQuestion1)
    {
        additionalQuestion1 = $"{additionalQuestion1}?";
        var vm = new TestDisplayVacancyViewModel
        {
            AdditionalQuestion2 = additionalQuestion1
        };

        vm.AdditionalQuestion2.Should().Be(additionalQuestion1);
    }
    
    [Test, AutoData]
    public void And_Has_Value_With_No_QuestionMark_Then_Returns_Value_With_QuestionMark(string additionalQuestion1)
    {
        var vm = new TestDisplayVacancyViewModel
        {
            AdditionalQuestion2 = additionalQuestion1
        };

        vm.AdditionalQuestion2.Should().Be($"{additionalQuestion1}?");
    }
}