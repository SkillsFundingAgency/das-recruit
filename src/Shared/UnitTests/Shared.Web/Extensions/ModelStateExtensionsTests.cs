using Esfa.Recruit.Shared.Web.Exceptions;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Extensions;

public class ModelStateExtensionsTests
{
    [Test]
    public void ThrowIfBindingErrors_Should_Not_Throw_If_No_ModelState()
    {
        // arrange
        ModelStateDictionary modelState = null;
        
        // act
        Action action = () => modelState.ThrowIfBindingErrors();
        
        // assert
        action.Should().NotThrow();
    }
    
    [Test]
    public void ThrowIfBindingErrors_Should_Not_Throw_If_ModelState_Is_Valid()
    {
        // arrange
        var modelState = new ModelStateDictionary();
        
        // act
        Action action = () => modelState.ThrowIfBindingErrors();
        
        // assert
        action.Should().NotThrow();
    }
    
    [Test]
    public void ThrowIfBindingErrors_Should_Throw_If_ModelState_Has_Errors()
    {
        // arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("fieldName", "An error message");
        
        // act
        Action action = () => modelState.ThrowIfBindingErrors();
        
        // assert
        action.Should()
            .Throw<ModelBindingException>()
            .Where(x => x.Message.Contains("fieldName"));
    }
}