using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.ViewModels.TaskList;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Domain.ProviderTaskListViewTests;

public class WhenGettingSectionSixState
{
    [TestCase(ApprenticeshipTypes.Standard)]
    [TestCase(ApprenticeshipTypes.Foundation)]
    public void If_Vacancy_Submitted_Section_Six_Should_Be_Complete(ApprenticeshipTypes apprenticeshipType)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = apprenticeshipType;
        vacancy.OwnerType = OwnerType.Provider;
        vacancy.Status = VacancyStatus.Submitted;
        var states = Enum.GetValues<TaskListItemFlags>().ToDictionary(f => f, _ => true);
        
        // act
        var sut = new ProviderTaskListStateView(states, vacancy);
        
        // assert
        sut.SectionSixState.Should().Be(VacancyTaskListSectionState.Completed);
    }
    
    [TestCase(ApprenticeshipTypes.Standard)]
    [TestCase(ApprenticeshipTypes.Foundation)]
    public void If_Vacancy_Not_Submitted_And_All_Sections_Complete_Section_Six_Should_Be_InProgress(ApprenticeshipTypes apprenticeshipType)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = apprenticeshipType;
        vacancy.OwnerType = OwnerType.Provider;
        vacancy.Status = VacancyStatus.Draft;
        var states = Enum.GetValues<TaskListItemFlags>().ToDictionary(f => f, _ => true);
        
        // act
        var sut = new ProviderTaskListStateView(states, vacancy);
        
        // assert
        sut.SectionSixState.Should().Be(VacancyTaskListSectionState.InProgress);
    }
    
    [TestCase(ApprenticeshipTypes.Standard)]
    [TestCase(ApprenticeshipTypes.Foundation)]
    public void If_Vacancy_Not_Submitted_And_Section_Five_Is_Not_Complete_Section_Six_Should_Be_NotStarted(ApprenticeshipTypes apprenticeshipType)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = apprenticeshipType;
        vacancy.OwnerType = OwnerType.Provider;
        vacancy.Status = VacancyStatus.Draft;
        var states = Enum.GetValues<TaskListItemFlags>().ToDictionary(f => f, _ => true);
        const TaskListItemFlags sectionFiveFlags = (TaskListItemFlags)ProviderTaskListSectionFlags.Five;
        foreach (var kvp in states.Where(kvp => sectionFiveFlags.HasFlag(kvp.Key)))
        {
            states[kvp.Key] = false;
        }
        
        // act
        var sut = new ProviderTaskListStateView(states, vacancy);
        
        // assert
        sut.SectionSixState.Should().Be(VacancyTaskListSectionState.NotStarted);
    }
    
    [TestCase(ApprenticeshipTypes.Standard)]
    [TestCase(ApprenticeshipTypes.Foundation)]
    public void If_Section_Five_Is_Complete_CheckYourAnswersEditable_Is_True(ApprenticeshipTypes apprenticeshipType)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = apprenticeshipType;
        vacancy.OwnerType = OwnerType.Provider;
        var states = Enum.GetValues<TaskListItemFlags>().ToDictionary(f => f, _ => true);
        
        // act
        var sut = new ProviderTaskListStateView(states, vacancy);
        
        // assert
        sut.CheckYourAnswersEditable.Should().BeTrue();
    }

    public static IEnumerable<object[]> GetAllIndividualFlagTestCases()
    {
        var result = Enum.GetValues<TaskListItemFlags>().ToList();
        result.Remove(TaskListItemFlags.All);
        result.Remove(TaskListItemFlags.None);
        result.Remove(TaskListItemFlags.OrganisationName); // Employer only
        result.Remove(TaskListItemFlags.TrainingProvider); // Employer only

        var standards = result.Select(x => new object[] { ApprenticeshipTypes.Standard, x });
        var foundations = result.Select(x => new object[] { ApprenticeshipTypes.Foundation, x });
        
        return standards.Concat(foundations);
    }

    [TestCaseSource(nameof(GetAllIndividualFlagTestCases))]
    public void If_Any_Flag_Is_Not_Complete_CheckYourAnswersEditable_Is_False(ApprenticeshipTypes apprenticeshipType, TaskListItemFlags flag)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = apprenticeshipType;
        vacancy.OwnerType = OwnerType.Provider;
        var states = Enum.GetValues<TaskListItemFlags>().ToDictionary(f => f, _ => true);
        states[flag] = false;
        
        // act
        var sut = new ProviderTaskListStateView(states, vacancy);
        
        // assert
        sut.CheckYourAnswersEditable.Should().BeFalse();
    }
}