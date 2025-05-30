﻿using Esfa.Recruit.Employer.Web.Models.AddLocation;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Models;

public class EnterLocationManuallyEditModelTests
{
    [Test]
    public void ToDomain_Humanizes_Address()
    {
        // arrange
        var sut = new EnterLocationManuallyEditModel
        {
            AddressLine1 = "1 somewhere road",
            AddressLine2 = "  ",
            City = "   nowhereton  ",
            Postcode = "ab1a 1aa",
        };
        
        // act
        var result = sut.ToDomain();

        // assert
        result.Should().NotBeNull();
        result.AddressLine1.Should().Be("1 Somewhere Road");
        result.AddressLine2.Should().BeEmpty();
        result.AddressLine3.Should().Be("Nowhereton");
        result.AddressLine4.Should().BeNull();
        result.Postcode.Should().Be("AB1A 1AA");
    }
}