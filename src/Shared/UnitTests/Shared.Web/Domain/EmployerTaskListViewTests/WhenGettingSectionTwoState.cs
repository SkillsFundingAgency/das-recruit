using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.ViewModels.TaskList;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Domain.EmployerTaskListViewTests;

public class WhenGettingSectionTwoState
{
    public static readonly IEnumerable<object[]> SectionTwoTestCases =
        new List<object[]>
        {
            new object[] { (TaskListItemFlags)ProviderTaskListSectionFlags.One, VacancyTaskListSectionState.NotStarted },
            new object[] { TaskListItemFlags.ClosingAndStartDates, VacancyTaskListSectionState.InProgress },
            new object[] { (TaskListItemFlags)ProviderTaskListSectionFlags.Two, VacancyTaskListSectionState.Completed },
        };
    
    [TestCaseSource(nameof(SectionTwoTestCases))]
    public void Section_Two_Should_Have_State(TaskListItemFlags flags, VacancyTaskListSectionState expectedState)
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
        sut.SectionTwoState.Should().Be(expectedState);
    }

    public static readonly IEnumerable<object[]> SectionTwoFieldsTestCases =
        new List<object[]>
        {
            new object[] { (EmployerTaskListStateView v) => v.ClosingAndStartDatesEditable, (TaskListItemFlags)ProviderTaskListSectionFlags.One },
            new object[] { (EmployerTaskListStateView v) => v.DurationAndWorkingHoursEditable, TaskListItemFlags.ClosingAndStartDates },
            new object[] { (EmployerTaskListStateView v) => v.PayRateAndBenefitsEditable, TaskListItemFlags.DurationAndWorkHours },
            new object[] { (EmployerTaskListStateView v) => v.NumberOfPositionsEditable, TaskListItemFlags.PayRateAndBenefits },
            new object[] { (EmployerTaskListStateView v) => v.LocationsEditable, TaskListItemFlags.NumberOfPositions },
        };
    
    [TestCaseSource(nameof(SectionTwoFieldsTestCases))]
    public void Section_Two_Fields_Require_Dependent_Fields(Func<EmployerTaskListStateView, bool> fieldFunc, TaskListItemFlags flags)
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