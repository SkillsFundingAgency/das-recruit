using System.Collections.Generic;
using System.Text.Encodings.Web;
using Esfa.Recruit.Shared.Web.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NUnit.Framework;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public class AddressListTagHelperTests
{
    private TagHelperContext _tagHelperContext;
    private TagHelperOutput _tagHelperOutput;
    
    [SetUp]
    public void SetUp()
    {
        _tagHelperContext = new TagHelperContext([], new Dictionary<object, object>(), "id");
        _tagHelperOutput = new TagHelperOutput(AddressListTagHelper.TagName, [], Func);
        return;

        static Task<TagHelperContent> Func(bool result, HtmlEncoder encoder)
        {
            var tagHelperContent = new DefaultTagHelperContent();
            tagHelperContent.SetHtmlContent(string.Empty);
            return Task.FromResult<TagHelperContent>(tagHelperContent);
        }
    }
    
    [Test]
    public async Task Output_Is_Suppressed_When_Address_Is_Null()
    {
        // arrange
        var sut = new AddressListTagHelper();

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().BeEmpty();
    }
    
    [TestCase("address1", "address2", "address3", "address4", "postcode", "<ul><li>address1, address2, address3, address4, postcode</li></ul>")]
    [TestCase("address1", null, "address3", "address4", "postcode", "<ul><li>address1, address3, address4, postcode</li></ul>")]
    [TestCase("address1", "address2", null, "address4", "postcode", "<ul><li>address1, address2, address4, postcode</li></ul>")]
    [TestCase("address1", "address2", "address3", null, "postcode", "<ul><li>address1, address2, address3, postcode</li></ul>")]
    [TestCase("address1", null, null, null, "postcode", "<ul><li>address1, postcode</li></ul>")]
    public async Task Renders_Single_Line_Address(string address1, string address2, string address3, string address4, string postcode, string output)
    {
        // arrange
        var sut = new AddressListTagHelper
        {
            Addresses = [new Address
            {
                AddressLine1 = address1,
                AddressLine2 = address2,
                AddressLine3 = address3,
                AddressLine4 = address4,
                Postcode = postcode
            }]
        };
    
        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);
    
        // assert
        _tagHelperOutput.AsString().Should().Be(output);
    }
    
    [Test]
    public async Task Renders_Anonymised_Address()
    {
        // arrange
        var sut = new AddressListTagHelper
        {
            Anonymised = true,
            Addresses = [new Address
            {
                AddressLine1 = "address1",
                AddressLine2 = "address2",
                AddressLine3 = "city",
                AddressLine4 = null,
                Postcode = "SW1A 2AA"
            }]
        };
    
        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);
    
        // assert
        _tagHelperOutput.AsString().Should().Be("<ul><li>city (SW1A)</li></ul>");
    }
    
    [Test]
    public async Task Renders_Grouped_Anonymised_Address()
    {
        // arrange
        var sut = new AddressListTagHelper
        {
            Anonymised = true,
            Addresses = [
                new Address
                {
                    AddressLine1 = "address1",
                    AddressLine4 = "city",
                    Postcode = "SW1A 2AA"
                },
                new Address
                {
                    AddressLine1 = "address1",
                    AddressLine3 = "city",
                    Postcode = "SW1A 2BB"
                }
            ]
        };
    
        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);
    
        // assert
        _tagHelperOutput.AsString().Should().Be("<ul><li>city (SW1A)</li></ul>");
    }
    
    [Test]
    public async Task Renders_Many_Addresses()
    {
        // arrange
        var sut = new AddressListTagHelper
        {
            Addresses = [
                new Address
                {
                    AddressLine1 = "address1",
                    AddressLine3 = "city1",
                    Postcode = "SW1A 2AA"
                },
                new Address
                {
                    AddressLine1 = "address1",
                    AddressLine3 = "city2",
                    Postcode = "SW1A 2AB"
                },
            ]
        };
    
        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);
    
        // assert
        _tagHelperOutput.AsString().Should().Be("<ul><li>address1, city1, SW1A 2AA</li><li>address1, city2, SW1A 2AB</li></ul>");
    }
    
    [Test]
    public async Task Renders_Abridged_Addresses()
    {
        // arrange
        var sut = new AddressListTagHelper
        {
            Abridged = true,
            Addresses = [
                new Address
                {
                    AddressLine1 = "address1",
                    AddressLine3 = "city1",
                    Postcode = "SW1A 2AA"
                },
                new Address
                {
                    AddressLine1 = "address1",
                    AddressLine3 = "city2",
                    Postcode = "SW1A 2AB"
                },
            ]
        };
    
        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);
    
        // assert
        _tagHelperOutput.AsString().Should().Be("<ul><li>city1 (SW1A 2AA)</li><li>city2 (SW1A 2AB)</li></ul>");
    }

    [Test]
    public async Task Classes_Are_Added_To_The_Tag()
    {
        // arrange
        var sut = new AddressListTagHelper
        {
            Class = "class1 class2",
            Addresses = [new Address { AddressLine1 = "address1", Postcode = "postcode" }]
        };

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().Be("<ul class=\"class1 class2\"><li>address1, postcode</li></ul>");
    }
}