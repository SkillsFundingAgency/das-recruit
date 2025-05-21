using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.ViewModels.TaskList;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Domain.EmployerTaskListViewTests;

public class WhenGettingSectionFiveState
{
    public static readonly IEnumerable<object[]> SectionFiveTestCases =
        new List<object[]>
        {
            new object[] { (TaskListItemFlags)EmployerTaskListSectionFlags.Four, VacancyTaskListSectionState.NotStarted },
            new object[] { TaskListItemFlags.AdditionalQuestions, VacancyTaskListSectionState.Completed },
            new object[] { (TaskListItemFlags)EmployerTaskListSectionFlags.Five, VacancyTaskListSectionState.Completed },
        };
    
    [TestCaseSource(nameof(SectionFiveTestCases))]
    public void Section_Five_Should_Have_State(TaskListItemFlags flags, VacancyTaskListSectionState expectedState)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
        vacancy.OwnerType = OwnerType.Employer;
        var states = Enum.GetValues<TaskListItemFlags>().ToDictionary(f => f, _ => false);
        foreach (var kvp in states.Where(kvp => flags.HasFlag(kvp.Key)))
        {
            states[kvp.Key] = true;
        }
        
        // act
        var sut = new EmployerTaskListStateView(states, vacancy);
        
        // assert
        sut.SectionFiveState.Should().Be(expectedState);
    }

    public static readonly IEnumerable<object[]> SectionFiveFieldsTestCases =
        new List<object[]>
        {
            new object[] { (EmployerTaskListStateView v) => v.AdditionalQuestionsEditable, (TaskListItemFlags)EmployerTaskListSectionFlags.Four },
        };
    
    [TestCaseSource(nameof(SectionFiveFieldsTestCases))]
    public void Section_Five_Fields_Require_Dependent_Fields(Func<EmployerTaskListStateView, bool> fieldFunc, TaskListItemFlags flags)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
        vacancy.OwnerType = OwnerType.Employer;
        var states = Enum.GetValues<TaskListItemFlags>().ToDictionary(f => f, _ => true);
        foreach (var kvp in states.Where(kvp => flags.HasFlag(kvp.Key)))
        {
            states[kvp.Key] = false;
        }
        
        // act
        var sut = new EmployerTaskListStateView(states, vacancy);
        bool actual = fieldFunc(sut);
        
        // assert
        actual.Should().BeFalse();
    }
}