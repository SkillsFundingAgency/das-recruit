﻿using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Extensions;

public class AddressExtensionsTests
{
    [Test]
    public void Null_List_Returns_Null_Result()
    {
        // arrange
        List<Address> sut = null;
        
        // act
        var result = sut.GroupByLastFilledAddressLine();

        // assert
        result.Should().BeNull();
    }
    
    [Test]
    public void Empty_List_Returns_Empty_Result()
    {
        // arrange
        var sut = new List<Address>();
        
        // act
        var result = sut.GroupByLastFilledAddressLine();

        // assert
        result.Should().BeEmpty();
    }
    
    [Test]
    public void Addresses_With_Different_Lines_Filled_Are_Grouped_Correctly()
    {
        // arrange
        var sut = new List<Address>
        {
            new() { AddressLine1 = "1 Somewhere", AddressLine2 = "London"},
            new() { AddressLine1 = "2 Somewhere", AddressLine3 = "London"},
            new() { AddressLine1 = "3 Somewhere", AddressLine4 = "London"},
        };
        
        // act
        var result = sut.GroupByLastFilledAddressLine().ToArray();
        var flattenedResults = result.SelectMany(x => x.Select(kvp => kvp.Value));

        // assert
        result.Should().HaveCount(1);
        flattenedResults.Should().BeEquivalentTo(sut);
    }
    
    [Test]
    public void Addresses_Should_Be_Grouped_Alphabetically()
    {
        // arrange
        var address1 = new Address { AddressLine1 = "3 Somewhere", AddressLine4 = "Warwick"};
        var address2 = new Address { AddressLine1 = "1 Somewhere", AddressLine2 = "Belfast"};
        var address3 = new Address { AddressLine1 = "2 Somewhere", AddressLine3 = "London"};
        
        var sut = new List<Address>
        {
            address1,
            address2,
            address3,
        };
        
        // act
        var result = sut.GroupByLastFilledAddressLine().ToArray();
        var flattenedResults = result.SelectMany(x => x.Select(kvp => kvp.Value)).ToArray();

        // assert
        result.Should().HaveCount(3);
        flattenedResults.First().Should().Be(address2);
        flattenedResults.Skip(1).First().Should().Be(address3);
        flattenedResults.Last().Should().Be(address1);
    }
    
    [Test]
    public void Addresses_Should_Be_Grouped()
    {
        // arrange
        var sut = new List<Address>
        {
            new() { AddressLine1 = "1 Somewhere", AddressLine3 = "London"},
            new() { AddressLine1 = "2 Somewhere", AddressLine3 = "London"},
            new() { AddressLine1 = "3 Somewhere", AddressLine3 = "Belfast"},
            new() { AddressLine1 = "4 Somewhere", AddressLine3 = "Belfast"},
            new() { AddressLine1 = "5 Somewhere", AddressLine3 = "Belfast"},
            new() { AddressLine1 = "6 Somewhere", AddressLine3 = "Edinburgh"},
            new() { AddressLine1 = "7 Somewhere", AddressLine3 = "Warwick"},
        };
        
        // act
        var result = sut.GroupByLastFilledAddressLine().ToArray().ToArray();

        // assert
        result.Should().HaveCount(4);
        result.First().Should().HaveCount(3);
        result.Skip(1).First().Should().HaveCount(1);
        result.Skip(2).First().Should().HaveCount(2);
        result.Last().Should().HaveCount(1);
    }
    
    [Test]
    public void Addresses_Should_Be_Grouped_Ignoring_Case()
    {
        // arrange
        var sut = new List<Address>
        {
            new() { AddressLine1 = "1 Somewhere", AddressLine3 = "London"},
            new() { AddressLine1 = "2 Somewhere", AddressLine3 = "london"},
            new() { AddressLine1 = "3 Somewhere", AddressLine3 = "place NAME"},
            new() { AddressLine1 = "4 Somewhere", AddressLine3 = "Place NaMe"},
            new() { AddressLine1 = "5 Somewhere", AddressLine3 = "PLACE name"},
        };
        
        // act
        var result = sut.GroupByLastFilledAddressLine().ToArray().ToArray();

        // assert
        result.Should().HaveCount(2);
        result.First().Should().HaveCount(2);
        result.Last().Should().HaveCount(3);
    }

    private static readonly IEnumerable<object[]> CityTestCases =
    [
        [ new Address { AddressLine1 = "AddressLine1" }, "AddressLine1" ],
        [ new Address { AddressLine1 = "AddressLine1", AddressLine2 = "AddressLine2" }, "AddressLine2" ],
        [ new Address { AddressLine1 = "AddressLine1", AddressLine2 = "AddressLine2", AddressLine3 = "AddressLine3" }, "AddressLine3" ],
        [ new Address { AddressLine1 = "AddressLine1", AddressLine2 = "AddressLine2", AddressLine3 = "AddressLine3", AddressLine4 = "AddressLine4" }, "AddressLine4" ],
    ];
        
    [TestCaseSource(nameof(CityTestCases))]
    public void City_Is_The_Last_Populated_Field(Address address, string expectedCity)
    {
        // act
        string result = address.GetLastNonEmptyField();
        
        // assert
        result.Should().Be(expectedCity);
    }

    [Test]
    public void SplitCitiesToList_ShouldReturnEmptyList_WhenInputIsNull()
    {
        string input = null;
        var result = input.SplitCitiesToList();
        result.Should().BeEmpty();
    }

    [Test]
    public void SplitCitiesToList_ShouldReturnEmptyList_WhenInputIsEmpty()
    {
        string input = "";
        var result = input.SplitCitiesToList();
        result.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public void SplitCitiesToList_ShouldReturnSingleCity_WhenInputHasOneCity(string cityName)
    {
        var result = cityName.SplitCitiesToList();
        result.Should().BeEquivalentTo(new List<string> { cityName });
    }

    [Test, MoqAutoData]
    public void SplitCitiesToList_ShouldReturnMultipleCities_WhenInputHasMultipleCities(string cityName1,
        string cityName2,
        string cityName3)
    {
        string input = $"{cityName1}, {cityName2}, {cityName3}";
        var result = input.SplitCitiesToList();
        result.Should().BeEquivalentTo(new List<string> { cityName1, cityName2, cityName3 });
    }

    [Test]
    public void SplitCitiesToList_ShouldTrimWhitespace_AroundCityNames()
    {
        string input = " London ,  Manchester ,Leeds ";
        var result = input.SplitCitiesToList();
        result.Should().BeEquivalentTo(new List<string> { "London", "Manchester", "Leeds" });
    }

    [Test]
    public void SplitCitiesToList_ShouldIgnoreEmptyEntries()
    {
        string input = "London, , Manchester,,Leeds, ";
        var result = input.SplitCitiesToList();
        result.Should().BeEquivalentTo(new List<string> { "London", "Manchester", "Leeds" });
    }

    [Test]
    public void GetCityDisplayList_When_Given_Null_ShouldReturnEmptyList_WhenNoAddresses()
    {
        // Arrange
        List<Address> addresses = null;

        // Act
        var result = addresses.GetCityDisplayList();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetCityDisplayList_ShouldReturnEmptyList_WhenNoAddresses()
    {
        // Arrange
        var addresses = new List<Address>();

        // Act
        var result = addresses.GetCityDisplayList();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetCityDisplayList_ShouldReturnSingleCity_WhenOneAddress()
    {
        // Arrange
        var addresses = new List<Address>
            {
                new Address { AddressLine2 = "Line2", AddressLine3 = "CityA" }
            };

        // Act
        var result = addresses.GetCityDisplayList();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("CityA");
    }

    [Test]
    public void GetCityDisplayList_ShouldReturnDistinctCities_WhenMultipleAddressesWithDifferentCities()
    {
        // Arrange
        var addresses = new List<Address>
            {
                new Address { AddressLine3 = "CityA" },
                new Address { AddressLine3 = "CityB" }
            };

        // Act
        var result = addresses.GetCityDisplayList();

        // Assert
        result.Should().BeEquivalentTo(new[] { "CityA", "CityB" });
    }

    [Test]
    public void GetCityDisplayList_ShouldReturnCityWithAddressLine1_WhenDuplicateCities()
    {
        // Arrange
        var addresses = new List<Address>
            {
                new Address { AddressLine1 = "Addr1", AddressLine3 = "CityA" },
                new Address { AddressLine1 = "Addr2", AddressLine3 = "CityA" }
            };

        // Act
        var result = addresses.GetCityDisplayList();

        // Assert
        result.Should().BeEquivalentTo(new[] { "CityA (Addr1)", "CityA (Addr2)" });
    }

    [Test]
    public void GetCityDisplayList_ShouldIgnoreAddressesWithNoCity()
    {
        // Arrange
        var addresses = new List<Address>
            {
                new Address { AddressLine1 = "Addr1", AddressLine3 = "" },
                new Address { AddressLine1 = "Addr2", AddressLine3 = "CityA" }
            };

        // Act
        var result = addresses.GetCityDisplayList();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("CityA");
    }

    [Test]
    public void GetCityDisplayList_ShouldTrimCityNamesAndAddressLine1()
    {
        // Arrange
        var addresses = new List<Address>
            {
                new Address { AddressLine1 = " Addr1 ", AddressLine3 = " CityA " },
                new Address { AddressLine1 = "Addr2", AddressLine3 = "CityA" }
            };

        // Act
        var result = addresses.GetCityDisplayList();

        // Assert
        result.Should().BeEquivalentTo("CityA (Addr1)", "CityA (Addr2)");
    }
}