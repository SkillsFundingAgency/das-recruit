using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
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
}