using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.Validations;

public class AddLocationEditModelValidatorTests
{
    [Test]
    public void Postcode_Cannot_Be_Null()
    {
        // arrange
        var model = new AddLocationEditModel { Postcode = null };
        
        // act
        var result = new AddLocationEditModelValidator().Validate(model);

        // assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorMessage.Should().Be(AddLocationEditModelValidator.NotNullErrorMessage);
    }
    
    [Test]
    public void Postcode_Cannot_Be_Greater_Than_Eight_Characters()
    {
        // arrange
        var model = new AddLocationEditModel { Postcode = "SW1A  2AA" };
        
        // act
        var result = new AddLocationEditModelValidator().Validate(model);

        // assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorMessage.Should().Be(AddLocationEditModelValidator.MaxLengthErrorMessage);
    }
    
    [TestCase("SW1A 2AA")]
    [TestCase("sW1A 2aa")]
    [TestCase("SW1A2AA")]
    [TestCase("SW1A 0AA")]
    [TestCase("SW1A0AA")]
    [TestCase("M11AA")]
    public void Postcode_Validates_Successfully(string input)
    {
        // arrange
        var model = new AddLocationEditModel { Postcode = input };
        
        // act
        var result = new AddLocationEditModelValidator().Validate(model);

        // assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }
    
    [TestCase("SW1A £AA")]
    [TestCase("SW1A^2AA")]
    [TestCase("sw")]
    [TestCase("SW1")]
    [TestCase("SW1A-2AA")]
    [TestCase("SW1A.2AA")]
    public void Invalid_Postcode_Fails_Validation(string input)
    {
        // arrange
        var model = new AddLocationEditModel { Postcode = input };
        
        // act
        var result = new AddLocationEditModelValidator().Validate(model);

        // assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorMessage.Should().Be(AddLocationEditModelValidator.InvalidPostcodeErrorMessage);
    }
}