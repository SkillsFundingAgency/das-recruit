using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.DisplayVacancy;

public class WhenGettingLocationDescription
{
    [TestCase(false, "SW1A 2AA", "CityName (SW1A 2AA)")]
    [TestCase(true, "SW1A", "CityName (SW1A)")]
    public void Then_Existing_Data_Is_Formatted_Correctly(bool isAnonymous, string postcode, string expectedDescription)
    {
        // arrange
        var sut = new TestDisplayVacancyViewModel
        {
            EmployerAddressElements = ["CityName", postcode],
            IsAnonymous = isAnonymous,
        };

        // act
        string result = sut.GetLocationDescription();

        // assert
        result.Should().Be(expectedDescription);
    }
    
    [TestCase(false, "Recruiting nationally")]
    [TestCase(true, "Recruiting nationally")]
    public void Then_Recruiting_Nationally_Is_Formatted_Correctly(bool isAnonymous, string expectedDescription)
    {
        // arrange
        var sut = new TestDisplayVacancyViewModel
        {
            AvailableWhere = AvailableWhere.AcrossEngland,
            IsAnonymous = isAnonymous,
        };

        // act
        string result = sut.GetLocationDescription();

        // assert
        result.Should().Be(expectedDescription);
    }
    
    [TestCase(false, "CityName (SW1A 2AA)")]
    [TestCase(true, "CityName (SW1A)")]
    public void Then_One_Location_Is_Formatted_Correctly(bool isAnonymous, string expectedDescription)
    {
        // arrange
        var sut = new TestDisplayVacancyViewModel
        {
            AvailableWhere = AvailableWhere.OneLocation,
            AvailableLocations = [ new Address { AddressLine4 = "CityName", Postcode = "SW1A 2AA" } ],
            IsAnonymous = isAnonymous
        };

        // act
        string result = sut.GetLocationDescription();

        // assert
        result.Should().Be(expectedDescription);
    }
    
    [TestCase(false, "ACityName, BCityName, CityName")]
    [TestCase(true, "ACityName, BCityName, CityName")]
    public void Then_Many_Locations_In_Different_Cities_Is_Formatted_Correctly(bool isAnonymous, string expectedDescription)
    {
        // arrange
        var sut = new TestDisplayVacancyViewModel
        {
            AvailableWhere = AvailableWhere.MultipleLocations,
            AvailableLocations = [
                new Address { AddressLine4 = "BCityName", Postcode = "EW1A 2AA" },
                new Address { AddressLine4 = "CityName", Postcode = "TW1A 2AB" },
                new Address { AddressLine4 = "ACityName", Postcode = "SW1A 2AC" },
                new Address { AddressLine4 = "ACityName", Postcode = "SW1A 2AD" },
                new Address { AddressLine4 = "ACityName", Postcode = "SW1A 2AE" },
                new Address { AddressLine4 = "ACityName", Postcode = "SW1B 2AE" }
            ],
            IsAnonymous = isAnonymous
        };

        // act
        string result = sut.GetLocationDescription();

        // assert
        result.Should().Be(expectedDescription);
    }
    
    [TestCase(false, "ACityName (3 available locations)")]
    [TestCase(true, "ACityName (3 available locations)")]
    public void Then_Many_Locations_In_Same_City_Is_Formatted_Correctly(bool isAnonymous, string expectedDescription)
    {
        // arrange
        var sut = new TestDisplayVacancyViewModel
        {
            AvailableWhere = AvailableWhere.MultipleLocations,
            AvailableLocations = [
                new Address { AddressLine4 = "ACityName", Postcode = "SW1A 2AC" },
                new Address { AddressLine4 = "ACityName", Postcode = "SW2A 2AD" },
                new Address { AddressLine4 = "ACityName", Postcode = "SW1A 2AE" }
            ],
            IsAnonymous = isAnonymous
        };

        // act
        string result = sut.GetLocationDescription();

        // assert
        result.Should().Be(expectedDescription);
    }
}