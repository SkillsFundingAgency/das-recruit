using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.ViewModels;

public class QualificationViewModelTests
{
    [TestCase(false, "B")]
    [TestCase(true, "B (predicted)")]
    public void GradeSummaryText_Is_Built_Corrently(bool isPredicted, string expected)
    {
        // arrange
        var viewModel = new QualificationViewModel
        {
            IsPredicted = isPredicted,
            Grade = "B"
        };
        viewModel.IsPredicted = isPredicted;
        
        // assert
        viewModel.GradeSummaryText.Should().Be(expected);
    }
}