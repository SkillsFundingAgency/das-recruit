using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
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

    [Test, MoqAutoData]
    public void GetCities_ReturnsEmptyString_WhenNoAddresses()
    {
        string result = new List<Address>().GetCities();

        result.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public void GetCities_ReturnsSingleCity_WhenOneAddress(
        Address address)
    {
        address.AddressLine2 = "Area";
        address.AddressLine3 = "CityA";
        address.Postcode = "AA1 1AA";
        address.AddressLine4 = string.Empty;
        var addresses = new List<Address> { address };

        string result = addresses.GetCities();

        result.Should().Be("CityA");
    }

    [Test, MoqAutoData]
    public void GetCities_ReturnsMultipleCities_WhenAddressesHaveDifferentCities(
        Address address1,
        Address address2)
    {
        address1.AddressLine3 = "CityA";
        address1.AddressLine1 = "Line1A";
        address1.Postcode = "AA1 1AA";
        address1.AddressLine4 = string.Empty;

        address2.AddressLine3 = "CityB";
        address2.AddressLine1 = "Line1B";
        address2.Postcode = "BB1 1BB";
        address2.AddressLine4 = string.Empty;
        var addresses = new List<Address> { address1, address2 };

        string result = addresses.GetCities();

        result.Should().Contain("CityA");
        result.Should().Contain("CityB");
        result.Should().NotContain("(");
    }

    [Test, MoqAutoData]
    public void GetCities_AppendsAddressLine1_WhenDuplicateCities(Address address1, Address address2)
    {
        address1.AddressLine3 = "CityA";
        address1.AddressLine1 = "Line1A";
        address1.Postcode = "AA1 1AA";
        address1.AddressLine4 = string.Empty;

        address2.AddressLine3 = "CityA";
        address2.AddressLine1 = "Line1B";
        address2.Postcode = "AA1 1AB";
        address2.AddressLine4 = string.Empty;
        var addresses = new List<Address> { address1, address2 };

        string result = addresses.GetCities();

        result.Should().Contain("CityA (Line1A)");
        result.Should().Contain("CityA (Line1B)");
    }

    [Test, MoqAutoData]
    public void GetCities_TrimsCityAndAddressLine1(Address address1, Address address2)
    {
        address1.AddressLine3 = "  CityA  ";
        address1.AddressLine1 = "  Line1A  ";
        address1.Postcode = "AA1 1AA";
        address1.AddressLine4 = string.Empty;

        address2.AddressLine3 = "CityA";
        address2.AddressLine1 = "Line1B";
        address2.Postcode = "AA1 1AB";
        address2.AddressLine4 = string.Empty;
        var addresses = new List<Address> { address1, address2 };

        string result = addresses.GetCities();

        result.Should().Contain("CityA (Line1A)");
        result.Should().Contain("CityA (Line1B)");
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
}