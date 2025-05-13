using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Extensions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Extensions;

public class EmailTemplateAddressExtensionTests
{
    public static object[][] TestCases => [
        [new Vacancy { EmployerLocationOption = null }, null],
        
        [new Vacancy { EmployerLocationOption = AvailableWhere.OneLocation }, null],
        [new Vacancy { EmployerLocationOption = AvailableWhere.MultipleLocations }, null],
        [new Vacancy { EmployerLocationOption = AvailableWhere.AcrossEngland }, "Recruiting nationally"],
        
        [new Vacancy { EmployerLocationOption = null, EmployerLocation = new Address { Postcode = "SW1A 2AA" } }, "SW1A 2AA"],
        [new Vacancy { EmployerLocationOption = null, EmployerLocation = new Address { AddressLine4 = "London", Postcode = "SW1A 2AA" } }, "London (SW1A 2AA)"],
        [new Vacancy { EmployerLocationOption = null, EmployerLocation = new Address { AddressLine3 = "London", Postcode = "SW1A 2AA" } }, "London (SW1A 2AA)"],
        [new Vacancy { EmployerLocationOption = null, EmployerLocation = new Address { AddressLine2 = "London", Postcode = "SW1A 2AA" } }, "London (SW1A 2AA)"],
        [new Vacancy { EmployerLocationOption = null, EmployerLocation = new Address { AddressLine1 = "London", Postcode = "SW1A 2AA" } }, "SW1A 2AA"],
        
        [new Vacancy { EmployerLocationOption = AvailableWhere.OneLocation, EmployerLocations = [new Address { AddressLine1 = "London", Postcode = "SW1A 2AA" }] }, "SW1A 2AA"],
        [new Vacancy { EmployerLocationOption = AvailableWhere.OneLocation, EmployerLocations = [new Address { AddressLine4 = "London", Postcode = "SW1A 2AA" }] }, "London (SW1A 2AA)"],
        [new Vacancy { EmployerLocationOption = AvailableWhere.OneLocation, EmployerLocations = [new Address { AddressLine3 = "London", Postcode = "SW1A 2AA" }] }, "London (SW1A 2AA)"],
        [new Vacancy { EmployerLocationOption = AvailableWhere.OneLocation, EmployerLocations = [new Address { AddressLine2 = "London", Postcode = "SW1A 2AA" }] }, "London (SW1A 2AA)"],
        [new Vacancy { EmployerLocationOption = AvailableWhere.OneLocation, EmployerLocations = [new Address { AddressLine1 = "London", Postcode = "SW1A 2AA" }] }, "SW1A 2AA"],
        
        [new Vacancy { EmployerLocationOption = AvailableWhere.MultipleLocations, EmployerLocations = [new Address { AddressLine4 = "London", Postcode = "SW1A 2AA" }, new Address { AddressLine4 = "Swindon", Postcode = "SN1 1BW" }] }, "London, Swindon"],
        [new Vacancy { EmployerLocationOption = AvailableWhere.MultipleLocations, EmployerLocations = [new Address { AddressLine4 = "London", Postcode = "SW1A 2AA" }, new Address { AddressLine4 = "London", Postcode = "SW1A 2AA" }] }, "London (2 available locations)"],
    ];
    
    [TestCaseSource(nameof(TestCases))]
    public void GetVacancyLocation_Should_Generate_The_Correct_Vacancy_Location_Description(Vacancy vacancy, string expected)
    {
        // act
        string result = vacancy.GetVacancyLocation();
        
        // assert
        result.Should().Be(expected);
    }
    
    [Test]
    public void GetVacancyLocation_Should_Sort_Multiple_Locations()
    {
        // arrange
        Vacancy vacancy = new Vacancy()
        {
            EmployerLocationOption = AvailableWhere.MultipleLocations,
            EmployerLocations = [
                new Address { AddressLine4 = "Swindon", Postcode = "SN1 1BW" },
                new Address { AddressLine4 = "London", Postcode = "SW1A 2AA" },
            ]
        };
        
        // act
        string result = vacancy.GetVacancyLocation();
        
        // assert
        result.Should().Be("London, Swindon");
    }
    
    [Test]
    public void GetVacancyLocation_Should_Dedupe_Multiple_Locations_Across_More_Than_One_City()
    {
        // arrange
        Vacancy vacancy = new Vacancy()
        {
            EmployerLocationOption = AvailableWhere.MultipleLocations,
            EmployerLocations = [
                new Address { AddressLine4 = "Swindon", Postcode = "SN2 1BW" },
                new Address { AddressLine4 = "Swindon", Postcode = "SN1 1BW" },
                new Address { AddressLine4 = "London", Postcode = "SW1A 2AA" },
            ]
        };
        
        // act
        string result = vacancy.GetVacancyLocation();
        
        // assert
        result.Should().Be("London, Swindon");
    }
    
    [Test]
    public void GetVacancyLocation_Should_Ensure_Only_Outcode_Is_Used_When_Vacancy_Is_Anonymous()
    {
        // arrange
        Vacancy vacancy = new Vacancy()
        {
            EmployerNameOption = EmployerNameOption.Anonymous,
            EmployerLocationOption = AvailableWhere.OneLocation,
            EmployerLocations = [
                new Address { AddressLine4 = "London", Postcode = "SW1A 2AA" },
            ]
        };
        
        // act
        string result = vacancy.GetVacancyLocation();
        
        // assert
        result.Should().Be("London (SW1A)");
    }
    
    [Test]
    public void GetVacancyLocation_Should_Ensure_Only_Outcode_Is_Used_When_Vacancy_Is_Anonymous_Using_Deprecated_Address()
    {
        // arrange
        Vacancy vacancy = new Vacancy()
        {
            EmployerNameOption = EmployerNameOption.Anonymous,
            EmployerLocationOption = null,
            EmployerLocation = new Address { AddressLine4 = "London", Postcode = "SW1A 2AA" },
        };
        
        // act
        string result = vacancy.GetVacancyLocation();
        
        // assert
        result.Should().Be("London (SW1A)");
    }
}