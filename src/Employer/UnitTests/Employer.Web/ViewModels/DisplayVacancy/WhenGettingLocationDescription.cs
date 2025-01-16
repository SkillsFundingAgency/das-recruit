using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.DisplayVacancy;

public class WhenGettingLocationDescription
{
    [Test]
    public void Then_Existing_Data_Is_Formatted_Correctly()
    {
        // arrange
        var sut = new TestDisplayVacancyViewModel
        {
            EmployerAddressElements = ["CityName", "SW1A 2AA"]
        };

        // act
        string result = sut.GetLocationDescription();

        // assert
        result.Should().Be("CityName (SW1A 2AA)");
    }
    
    [Test]
    public void Then_Recruiting_Nationally_Is_Formatted_Correctly()
    {
        // arrange
        var sut = new TestDisplayVacancyViewModel { AvailableWhere = AvailableWhere.AcrossEngland };

        // act
        string result = sut.GetLocationDescription();

        // assert
        result.Should().Be("Recruiting nationally");
    }
    
    [Test]
    public void Then_One_Location_Is_Formatted_Correctly()
    {
        // arrange
        var sut = new TestDisplayVacancyViewModel
        {
            AvailableWhere = AvailableWhere.OneLocation,
            AvailableLocations = [ new Address { AddressLine4 = "CityName", Postcode = "SW1A 2AA" } ]
        };

        // act
        string result = sut.GetLocationDescription();

        // assert
        result.Should().Be("CityName (SW1A 2AA)");
    }
    
    [Test]
    public void Then_Many_Locations_In_Different_Cities_Is_Formatted_Correctly()
    {
        // arrange
        var sut = new TestDisplayVacancyViewModel
        {
            AvailableWhere = AvailableWhere.MultipleLocations,
            AvailableLocations = [
                new Address { AddressLine4 = "BCityName", Postcode = "SW1A 2AA" },
                new Address { AddressLine4 = "CityName", Postcode = "SW1A 2AB" },
                new Address { AddressLine4 = "ACityName", Postcode = "SW1A 2AC" },
                new Address { AddressLine4 = "ACityName", Postcode = "SW1A 2AD" },
                new Address { AddressLine4 = "ACityName", Postcode = "SW1A 2AE" }
            ]
        };

        // act
        string result = sut.GetLocationDescription();

        // assert
        result.Should().Be("ACityName, BCityName, CityName");
    }
    
    [Test]
    public void Then_Many_Locations_In_Same_City_Is_Formatted_Correctly()
    {
        // arrange
        var sut = new TestDisplayVacancyViewModel
        {
            AvailableWhere = AvailableWhere.MultipleLocations,
            AvailableLocations = [
                new Address { AddressLine4 = "ACityName", Postcode = "SW1A 2AC" },
                new Address { AddressLine4 = "ACityName", Postcode = "SW1A 2AD" },
                new Address { AddressLine4 = "ACityName", Postcode = "SW1A 2AE" }
            ]
        };

        // act
        string result = sut.GetLocationDescription();

        // assert
        result.Should().Be("ACityName (3 available locations)");
    }
}