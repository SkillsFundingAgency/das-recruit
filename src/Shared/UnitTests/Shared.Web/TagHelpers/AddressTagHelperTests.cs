using System.Collections.Generic;
using System.Text.Encodings.Web;
using Esfa.Recruit.Shared.Web.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NUnit.Framework;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.TagHelpers;

public class AddressTagHelperTests
{
    private TagHelperContext _tagHelperContext;
    private TagHelperOutput _tagHelperOutput;
    
    [SetUp]
    public void SetUp()
    {
        _tagHelperContext = new TagHelperContext([], new Dictionary<object, object>(), "id");
        _tagHelperOutput = new TagHelperOutput(AddressTagHelper.TagName, [], Func);
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
        var sut = new AddressTagHelper();

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().BeEmpty();
    }
    
    [TestCase("address1", "address2", "address3", "address4", "postcode", "<p>address1<br>address2<br>address3<br>address4<br>postcode</p>")]
    [TestCase("address1", null, "address3", "address4", "postcode", "<p>address1<br>address3<br>address4<br>postcode</p>")]
    [TestCase("address1", "address2", null, "address4", "postcode", "<p>address1<br>address2<br>address4<br>postcode</p>")]
    [TestCase("address1", "address2", "address3", null, "postcode", "<p>address1<br>address2<br>address3<br>postcode</p>")]
    [TestCase("address1", null, null, null, "postcode", "<p>address1<br>postcode</p>")]
    public async Task Renders_Multi_Line_Address(string address1, string address2, string address3, string address4, string postcode, string output)
    {
        // arrange
        var sut = new AddressTagHelper
        {
            Value = new Address
            {
                AddressLine1 = address1,
                AddressLine2 = address2,
                AddressLine3 = address3,
                AddressLine4 = address4,
                Postcode = postcode
            }
        };

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().Be(output);
    }
    
    [TestCase("address1", "address2", "address3", "address4", "postcode", "<span>address1, address2, address3, address4, postcode</span>")]
    [TestCase("address1", null, "address3", "address4", "postcode", "<span>address1, address3, address4, postcode</span>")]
    [TestCase("address1", "address2", null, "address4", "postcode", "<span>address1, address2, address4, postcode</span>")]
    [TestCase("address1", "address2", "address3", null, "postcode", "<span>address1, address2, address3, postcode</span>")]
    [TestCase("address1", null, null, null, "postcode", "<span>address1, postcode</span>")]
    public async Task Renders_Single_Line_Address(string address1, string address2, string address3, string address4, string postcode, string output)
    {
        // arrange
        var sut = new AddressTagHelper
        {
            Flat = true,
            Value = new Address
            {
                AddressLine1 = address1,
                AddressLine2 = address2,
                AddressLine3 = address3,
                AddressLine4 = address4,
                Postcode = postcode
            }
        };

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().Be(output);
    }
    
    [TestCase(true, "<span>address4 (SW1A)</span>")]
    [TestCase(false, "<p>address4 (SW1A)</p>")]
    public async Task Renders_Anonymised_Address(bool flat, string output)
    {
        // arrange
        var sut = new AddressTagHelper
        {
            Anonymised = true,
            Flat = flat,
            Value = new Address
            {
                AddressLine1 = "address1",
                AddressLine2 = "address2",
                AddressLine3 = "address3",
                AddressLine4 = "address4",
                Postcode = "SW1A 2AA"
            }
        };

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().Be(output);
    }

    [Test]
    public async Task Classes_Are_Added_To_The_Tag()
    {
        // arrange
        var sut = new AddressTagHelper
        {
            Class = "class1 class2",
            Value = new Address { AddressLine1 = "address1", Postcode = "postcode" }
        };

        // act
        await sut.ProcessAsync(_tagHelperContext, _tagHelperOutput);

        // assert
        _tagHelperOutput.AsString().Should().Be("<p class=\"class1 class2\">address1<br>postcode</p>");
    }
}