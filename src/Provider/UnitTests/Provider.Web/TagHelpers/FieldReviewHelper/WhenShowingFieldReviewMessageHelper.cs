using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.TagHelpers;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.TagHelpers.FieldReviewHelper
{
    public class WhenShowingFieldReviewMessageHelper
    {
        [Test, MoqAutoData]
        public void Then_If_There_Are_No_Errors_Then_No_Message_Created(
            [Frozen] Mock<TagHelperContext> context, 
            FieldReviewMessageHelper fieldReviewMessageHelper)
        {
            fieldReviewMessageHelper.Model = new VacancyPreviewViewModel
            {
                Review = new ReviewSummaryViewModel()
            };
            var tagHelperOutput = new TagHelperOutput("", new TagHelperAttributeList(new List<TagHelperAttribute>()),
                (b, encoder) => null);
            
            fieldReviewMessageHelper.Process(context.Object, tagHelperOutput);

            tagHelperOutput.Content.IsModified.Should().BeFalse();
            tagHelperOutput.Content.IsEmptyOrWhiteSpace.Should().BeTrue();
        }

        [Test, MoqAutoData]
        public void Then_If_There_Are_Review_Fields_Then_Returned_In_Content(
            ReviewSummaryViewModel model,
            [Frozen] Mock<TagHelperContext> context, 
            FieldReviewMessageHelper fieldReviewMessageHelper)
        {
            
            fieldReviewMessageHelper.Model = new VacancyPreviewViewModel
            {
                Review = model
            };
            fieldReviewMessageHelper.FieldName = model.FieldIndicators.FirstOrDefault().ReviewFieldIdentifier;
            var tagHelperOutput = new TagHelperOutput("", new TagHelperAttributeList(new List<TagHelperAttribute>()),
                (b, encoder) => null);
            
            fieldReviewMessageHelper.Process(context.Object, tagHelperOutput);

            tagHelperOutput.Content.IsModified.Should().BeTrue();
            tagHelperOutput.Content.IsEmptyOrWhiteSpace.Should().BeFalse();
            tagHelperOutput.Content.GetContent().Should().Be(model.FieldIndicators.FirstOrDefault().ManualQaText);
        }

        [Test, MoqAutoData]
        public void Then_If_There_Are_Validation_Errors_Then_Returned_In_Content(
            string errorMessage,
            ReviewSummaryViewModel model,
            [Frozen] Mock<TagHelperContext> context, 
            FieldReviewMessageHelper fieldReviewMessageHelper)
        {
            var modelState = new ModelStateDictionary();
            modelState.AddModelError(model.FieldIndicators.FirstOrDefault().ReviewFieldIdentifier,errorMessage);
            fieldReviewMessageHelper.Model = new VacancyPreviewViewModel
            {
                ValidationErrors = new ValidationSummaryViewModel
                    {ModelState = modelState, OrderedFieldNames = new List<string>()}
            };
            fieldReviewMessageHelper.FieldName = model.FieldIndicators.FirstOrDefault().ReviewFieldIdentifier;
            var tagHelperOutput = new TagHelperOutput("", new TagHelperAttributeList(new List<TagHelperAttribute>()),
                (b, encoder) => null);
            
            fieldReviewMessageHelper.Process(context.Object, tagHelperOutput);

            tagHelperOutput.Content.IsModified.Should().BeTrue();
            tagHelperOutput.Content.IsEmptyOrWhiteSpace.Should().BeFalse();
            tagHelperOutput.Content.GetContent().Should().Be(errorMessage);
        }

        [Test, MoqAutoData]
        public void Then_If_There_Are_Validation_Errors_And_Review_Field_Errors_Then_Returned_In_Content(
            string errorMessage,
            ReviewSummaryViewModel model,
            [Frozen] Mock<TagHelperContext> context, 
            FieldReviewMessageHelper fieldReviewMessageHelper)
        {
            var modelState = new ModelStateDictionary();
            modelState.AddModelError(model.FieldIndicators.FirstOrDefault().ReviewFieldIdentifier,errorMessage);
            fieldReviewMessageHelper.Model = new VacancyPreviewViewModel
            {
                Review = model,
                ValidationErrors = new ValidationSummaryViewModel
                    {ModelState = modelState, OrderedFieldNames = new List<string>()}
            };
            fieldReviewMessageHelper.FieldName = model.FieldIndicators.FirstOrDefault().ReviewFieldIdentifier;
            var tagHelperOutput = new TagHelperOutput("", new TagHelperAttributeList(new List<TagHelperAttribute>()),
                (b, encoder) => null);
            
            fieldReviewMessageHelper.Process(context.Object, tagHelperOutput);

            tagHelperOutput.Content.IsModified.Should().BeTrue();
            tagHelperOutput.Content.IsEmptyOrWhiteSpace.Should().BeFalse();
            tagHelperOutput.Content.GetContent().Should().Be(errorMessage + "<br/>" +model.FieldIndicators.FirstOrDefault().ManualQaText);
        }
    }
}