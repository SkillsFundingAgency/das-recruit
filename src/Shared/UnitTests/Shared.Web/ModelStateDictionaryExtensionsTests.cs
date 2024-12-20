using System.Collections.Generic;
using Esfa.Recruit.Shared.Web;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web;

public class ModelStateDictionaryExtensionsTests
{
    [Test]
    public void Throws_When_ModelState_Is_Null()
    {
        // arrange
        var act = () => ((ModelStateDictionary)null).AddValidationErrors(new EntityValidationResult { Errors = [new EntityValidationError(1, "propertyName", "errorMessage", "errorCode")] });
        
        // act/assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*modelState*");
    }
    
    [Test]
    public void Throws_When_ValidationResult_Is_Null()
    {
        // arrange
        var dict = new ModelStateDictionary();
        var act = () => dict.AddValidationErrors(null);
        
        // act/assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*validationResult*");
    }
    
    [Test]
    public void Adds_ValidationResult_Errors_To_ModelState_Unmapped()
    {
        // arrange
        var dict = new ModelStateDictionary();
        var validationResult = new EntityValidationResult
        {
            Errors =
            [
                new EntityValidationError(1, "property1", "errorMessage1", "errorCode"),
                new EntityValidationError(1, "property1", "errorMessage2", "errorCode"),
                new EntityValidationError(1, "property2", "errorMessage3", "errorCode"),
            ]
        };
        
        // act
        dict.AddValidationErrors(validationResult);
        
        // assert
        dict.Should().HaveCount(2);
        dict["property1"]!.Errors.Should().HaveCount(2);
        dict["property1"]!.Errors[0].ErrorMessage.Should().Be("errorMessage1");
        dict["property1"]!.Errors[1].ErrorMessage.Should().Be("errorMessage2");
        dict["property2"]!.Errors.Should().HaveCount(1);
        dict["property2"]!.Errors[0].ErrorMessage.Should().Be("errorMessage3");
    }
    
    [Test]
    public void Adds_ValidationResult_Errors_To_ModelState_Mapped()
    {
        // arrange
        var dict = new ModelStateDictionary();
        var validationResult = new EntityValidationResult
        {
            Errors =
            [
                new EntityValidationError(1, "property1", "errorMessage1", "errorCode"),
                new EntityValidationError(1, "property1", "errorMessage2", "errorCode"),
                new EntityValidationError(1, "property2", "errorMessage3", "errorCode"),
            ]
        };
        var propertyMappings = new Dictionary<string, string>
        {
            {"property1", "mappedProperty1"},
            {"property2", "mappedProperty2"},
        };
        
        // act
        dict.AddValidationErrors(validationResult, propertyMappings);
        
        // assert
        dict.Should().HaveCount(2);
        dict["mappedProperty1"]!.Errors.Should().HaveCount(2);
        dict["mappedProperty1"]!.Errors[0].ErrorMessage.Should().Be("errorMessage1");
        dict["mappedProperty1"]!.Errors[1].ErrorMessage.Should().Be("errorMessage2");
        dict["mappedProperty2"]!.Errors.Should().HaveCount(1);
        dict["mappedProperty2"]!.Errors[0].ErrorMessage.Should().Be("errorMessage3");
    }
    
    [Test]
    public void Unmapped_Properties_Are_Still_Added_With_Default_Property_Name()
    {
        // arrange
        var dict = new ModelStateDictionary();
        var validationResult = new EntityValidationResult
        {
            Errors =
            [
                new EntityValidationError(1, "property1", "errorMessage1", "errorCode"),
                new EntityValidationError(1, "property1", "errorMessage2", "errorCode"),
                new EntityValidationError(1, "property2", "errorMessage3", "errorCode"),
            ]
        };
        var propertyMappings = new Dictionary<string, string>
        {
            {"property1", "mappedProperty1"},
        };
        
        // act
        dict.AddValidationErrors(validationResult, propertyMappings);
        
        // assert
        dict.Should().HaveCount(2);
        dict["mappedProperty1"]!.Errors.Should().HaveCount(2);
        dict["property2"]!.Errors.Should().HaveCount(1);
    }
}