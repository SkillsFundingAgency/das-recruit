using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.ViewModels.TaskList;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Domain.ProviderTaskListViewTests;

public class WhenGettingSectionFourState
{
    public static readonly IEnumerable<object[]> SectionFourTestCases =
        new List<object[]>
        {
            new object[] { (TaskListItemFlags)ProviderTaskListSectionFlags.Three, VacancyTaskListSectionState.NotStarted },
            new object[] { TaskListItemFlags.NameOfEmployerOnAdvert, VacancyTaskListSectionState.InProgress },
            new object[] { (TaskListItemFlags)ProviderTaskListSectionFlags.Four, VacancyTaskListSectionState.Completed },
        };
    
    [TestCaseSource(nameof(SectionFourTestCases))]
    public void Section_Four_Should_Have_State(TaskListItemFlags flags, VacancyTaskListSectionState expectedState)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
        vacancy.OwnerType = OwnerType.Provider;
        var states = Enum.GetValues<TaskListItemFlags>().ToDictionary(f => f, _ => false);
        foreach (var kvp in states.Where(kvp => flags.HasFlag(kvp.Key)))
        {
            states[kvp.Key] = true;
        }
        
        // act
        var sut = new ProviderTaskListStateView(states, vacancy);
        
        // assert
        sut.SectionFourState.Should().Be(expectedState);
    }

    public static readonly IEnumerable<object[]> SectionFourFieldsTestCases =
        new List<object[]>
        {
            new object[] { (ProviderTaskListStateView v) => v.NameOfEmployerOnVacancyEditable, (TaskListItemFlags)ProviderTaskListSectionFlags.Three | TaskListItemFlags.NameOfEmployerOnAdvert },
            new object[] { (ProviderTaskListStateView v) => v.EmployerInformationEditable, TaskListItemFlags.NameOfEmployerOnAdvert },
            new object[] { (ProviderTaskListStateView v) => v.ContactDetailsEditable, TaskListItemFlags.EmployerInformation },
            new object[] { (ProviderTaskListStateView v) => v.WebsiteForApplicationsEditable, TaskListItemFlags.EmployerInformation },
        };
    
    [TestCaseSource(nameof(SectionFourFieldsTestCases))]
    public void Section_Four_Fields_Require_Dependent_Fields(Func<ProviderTaskListStateView, bool> fieldFunc, TaskListItemFlags flags)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
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