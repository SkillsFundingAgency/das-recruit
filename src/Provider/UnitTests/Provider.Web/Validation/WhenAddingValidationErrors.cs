using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using FluentValidation.Results;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Validation;

public class WhenAddingValidationErrors
{
    [Test, MoqAutoData]
    public void Then_The_Validation_Failures_Are_Converted_To_Entity_Validation_Errors(List<ValidationFailure> errors)
    {
        // arrange
        List<EntityValidationError> validationErrors = [];

        // act
        validationErrors.AddValidationErrors(errors);

        // assert
        validationErrors.Should().HaveCount(errors.Count);
        validationErrors.Should().AllSatisfy(x =>
        {
            errors.SingleOrDefault(e => 
                e.ErrorMessage == x.ErrorMessage
                && e.ErrorCode == x.ErrorCode
                && e.PropertyName == x.PropertyName).Should().NotBeNull();
        });
    }
    
    [Test, MoqAutoData]
    public void Then_The_Validation_Failures_Custom_State_Is_Converted_To_The_RuleId_When_Set_To_A_Number(ValidationFailure error)
    {
        // arrange
        List<EntityValidationError> validationErrors = [];
        error.CustomState = 5L;

        // act
        validationErrors.AddValidationErrors([error]);

        // assert
        validationErrors.Should().HaveCount(1);
        validationErrors[0].RuleId.Should().Be(5);
    }
}