using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.ViewModels.TaskList;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Domain.ProviderTaskListViewTests;

public class WhenGettingSectionThreeState
{
    public static readonly IEnumerable<object[]> SectionThreeTestCases =
        new List<object[]>
        {
            new object[] { ApprenticeshipTypes.Standard, (TaskListItemFlags)ProviderTaskListSectionFlags.Two, VacancyTaskListSectionState.NotStarted },
            new object[] { ApprenticeshipTypes.Standard, TaskListItemFlags.Skills, VacancyTaskListSectionState.InProgress },
            new object[] { ApprenticeshipTypes.Foundation, TaskListItemFlags.FutureProspects, VacancyTaskListSectionState.InProgress },
            new object[] { ApprenticeshipTypes.Standard, (TaskListItemFlags)ProviderTaskListSectionFlags.Three, VacancyTaskListSectionState.Completed },
            new object[] { ApprenticeshipTypes.Foundation, (TaskListItemFlags)ProviderTaskListSectionFlags.Three, VacancyTaskListSectionState.Completed },
        };
    
    [TestCaseSource(nameof(SectionThreeTestCases))]
    public void Section_Three_Should_Have_State(ApprenticeshipTypes apprenticeshipType, TaskListItemFlags flags, VacancyTaskListSectionState expectedState)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = apprenticeshipType;
        vacancy.OwnerType = OwnerType.Provider;
        var states = Enum.GetValues<TaskListItemFlags>().ToDictionary(f => f, _ => false);
        foreach (var kvp in states.Where(kvp => flags.HasFlag(kvp.Key)))
        {
            states[kvp.Key] = true;
        }
        
        // act
        var sut = new ProviderTaskListStateView(states, vacancy);
        
        // assert
        sut.SectionThreeState.Should().Be(expectedState);
    }

    public static readonly IEnumerable<object[]> SectionThreeFieldsTestCases =
        new List<object[]>
        {
            new object[] { ApprenticeshipTypes.Standard, (ProviderTaskListStateView v) => v.SkillsEditable, (TaskListItemFlags)ProviderTaskListSectionFlags.Two },
            new object[] { ApprenticeshipTypes.Standard, (ProviderTaskListStateView v) => v.QualificationsEditable, TaskListItemFlags.Skills },
            new object[] { ApprenticeshipTypes.Standard, (ProviderTaskListStateView v) => v.FutureProspectsEditable, TaskListItemFlags.Qualifications | TaskListItemFlags.FutureProspects },
            new object[] { ApprenticeshipTypes.Standard, (ProviderTaskListStateView v) => v.OtherThingsToConsiderEditable, TaskListItemFlags.FutureProspects },
            new object[] { ApprenticeshipTypes.Foundation, (ProviderTaskListStateView v) => v.FutureProspectsEditable, (TaskListItemFlags)ProviderTaskListSectionFlags.Two },
            new object[] { ApprenticeshipTypes.Foundation, (ProviderTaskListStateView v) => v.OtherThingsToConsiderEditable, TaskListItemFlags.FutureProspects },
        };
    
    [TestCaseSource(nameof(SectionThreeFieldsTestCases))]
    public void Section_Three_Fields_Require_Dependent_Fields(ApprenticeshipTypes apprenticeshipType, Func<ProviderTaskListStateView, bool> fieldFunc, TaskListItemFlags flags)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = apprenticeshipType;
        vacancy.OwnerType = OwnerType.Provider;
        var states = Enum.GetValues<TaskListItemFlags>().ToDictionary(f => f, _ => true);
        foreach (var kvp in states.Where(kvp => flags.HasFlag(kvp.Key)))
        {
            states[kvp.Key] = false;
        }
        
        // act
        var sut = new ProviderTaskListStateView(states, vacancy);
        bool actual = fieldFunc(sut);
        
        // assert
        actual.Should().BeFalse();
    }
}