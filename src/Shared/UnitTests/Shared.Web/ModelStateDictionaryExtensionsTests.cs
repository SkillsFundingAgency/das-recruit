using System.Collections.Generic;
using Esfa.Recruit.Shared.Web;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web;

public class ModelStateDictionaryExtensionsTests
{
    [Test]
    public void AddValidationErrorsWithFieldMappings_Throws_When_ModelState_Is_Null()
    {
        // arrange
        var act = () => ((ModelStateDictionary)null).AddValidationErrorsWithFieldMappings(new EntityValidationResult { Errors = [new EntityValidationError(1, "propertyName", "errorMessage", "errorCode")] }, null);
        
        // act/assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*modelState*");
    }
    
    [Test]
    public void AddValidationErrorsWithFieldMappings_Throws_When_ValidationResult_Is_Null()
    {
        // arrange
        var dict = new ModelStateDictionary();
        var act = () => dict.AddValidationErrorsWithFieldMappings(null, null);
        
        // act/assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*validationResult*");
    }
    
    [Test]
    public void AddValidationErrorsWithFieldMappings_Adds_ValidationResult_Errors_To_ModelState_Unmapped()
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
        dict.AddValidationErrorsWithFieldMappings(validationResult, null);
        
        // assert
        dict.Should().HaveCount(2);
        dict["property1"]!.Errors.Should().HaveCount(2);
        dict["property1"]!.Errors[0].ErrorMessage.Should().Be("errorMessage1");
        dict["property1"]!.Errors[1].ErrorMessage.Should().Be("errorMessage2");
        dict["property2"]!.Errors.Should().HaveCount(1);
        dict["property2"]!.Errors[0].ErrorMessage.Should().Be("errorMessage3");
    }
    
    [Test]
    public void AddValidationErrorsWithFieldMappings_Adds_ValidationResult_Errors_To_ModelState_Mapped()
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
        dict.AddValidationErrorsWithFieldMappings(validationResult, propertyMappings);
        
        // assert
        dict.Should().HaveCount(2);
        dict["mappedProperty1"]!.Errors.Should().HaveCount(2);
        dict["mappedProperty1"]!.Errors[0].ErrorMessage.Should().Be("errorMessage1");
        dict["mappedProperty1"]!.Errors[1].ErrorMessage.Should().Be("errorMessage2");
        dict["mappedProperty2"]!.Errors.Should().HaveCount(1);
        dict["mappedProperty2"]!.Errors[0].ErrorMessage.Should().Be("errorMessage3");
    }
    
    [Test]
    public void AddValidationErrorsWithFieldMappings_Unmapped_Properties_Are_Still_Added_With_Default_Property_Name()
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
        dict.AddValidationErrorsWithFieldMappings(validationResult, propertyMappings);
        
        // assert
        dict.Should().HaveCount(2);
        dict["mappedProperty1"]!.Errors.Should().HaveCount(2);
        dict["property2"]!.Errors.Should().HaveCount(1);
    }
    
    // ========================================================================================================================
    // ========================================================================================================================
    // ========================================================================================================================
    // ========================================================================================================================
    // ========================================================================================================================
    // ========================================================================================================================
    // ========================================================================================================================
    // ========================================================================================================================
    // ========================================================================================================================
    //
    [Test]
    public void AddValidationErrorsWithMappings_Throws_When_ModelState_Is_Null()
    {
        // arrange
        var act = () => ((ModelStateDictionary)null).AddValidationErrorsWithMappings(new EntityValidationResult { Errors = [new EntityValidationError(1, "propertyName", "errorMessage", "errorCode")] }, null);
        
        // act/assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*modelState*");
    }
    
    [Test]
    public void AddValidationErrorsWithMappings_Throws_When_ValidationResult_Is_Null()
    {
        // arrange
        var dict = new ModelStateDictionary();
        var act = () => dict.AddValidationErrorsWithMappings(null, null);
        
        // act/assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*validationResult*");
    }

    [Test]
    public void AddValidationErrorsWithMappings_Adds_ValidationResult_Errors_To_ModelState_Unmapped()
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
        dict.AddValidationErrorsWithMappings(validationResult, null);
        
        // assert
        dict.Should().HaveCount(2);
        dict["property1"]!.Errors.Should().HaveCount(2);
        dict["property1"]!.Errors[0].ErrorMessage.Should().Be("errorMessage1");
        dict["property1"]!.Errors[1].ErrorMessage.Should().Be("errorMessage2");
        dict["property2"]!.Errors.Should().HaveCount(1);
        dict["property2"]!.Errors[0].ErrorMessage.Should().Be("errorMessage3");
    }
    
    [Test]
    public void AddValidationErrorsWithMappings_Adds_ValidationResult_Errors_To_ModelState_Mapped()
    {
        // arrange
        var dict = new ModelStateDictionary();
        var validationResult = new EntityValidationResult
        {
            Errors =
            [
                new EntityValidationError(1, "property1", "errorMessage1", "errorCode"),
                new EntityValidationError(1, "property2", "errorMessage2", "errorCode"),
            ]
        };
        var propertyMappings = new Dictionary<string, Tuple<string, string>>
        {
            {"property1", Tuple.Create("mappedProperty1", "mappedErrorMessage1") }
        };
        
        // act
        dict.AddValidationErrorsWithMappings(validationResult, propertyMappings);
        
        // assert
        dict.Should().HaveCount(2);
        dict["mappedProperty1"]!.Errors.Should().HaveCount(1);
        dict["mappedProperty1"]!.Errors[0].ErrorMessage.Should().Be("mappedErrorMessage1");
        dict["property2"]!.Errors.Should().HaveCount(1);
        dict["property2"]!.Errors[0].ErrorMessage.Should().Be("errorMessage2");
    }
}