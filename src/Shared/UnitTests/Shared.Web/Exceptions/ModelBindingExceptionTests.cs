using Esfa.Recruit.Shared.Web.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Exceptions;

public class ModelBindingExceptionTests
{
    [Test]
    public void ModelBindingException_Should_Accept_Null_Model_State()
    {
        // act
        Action action = () => throw new ModelBindingException(null);
        
        // assert
        action.Should().Throw<ModelBindingException>();
    }
    
    [Test]
    public void ModelBindingException_Should_Generate_Summary_Message()
    {
        // arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("firstField", "First error message");
        modelState.AddModelError("secondField", "Second error message");
        
        // act
        var sut = new ModelBindingException(modelState);
        string summary = sut.Message;
        
        // assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("firstField");
        summary.Should().Contain("First error message");
        summary.Should().Contain("secondField");
        summary.Should().Contain("Second error message");
    }
}