using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Shared.Web.ViewModels.TaskList;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Domain.ProviderTaskListViewTests;

public class WhenGettingSectionOneState
{
    public static readonly IEnumerable<object[]> SectionOneTestCases =
        new List<object[]>
        {
            new object[] { TaskListItemFlags.None, VacancyTaskListSectionState.NotStarted },
            new object[] { TaskListItemFlags.NameOfEmployer, VacancyTaskListSectionState.InProgress },
            new object[] { (TaskListItemFlags)ProviderTaskListSectionFlags.One, VacancyTaskListSectionState.Completed },
        };
    
    [TestCaseSource(nameof(SectionOneTestCases))]
    public void Section_One_Should_Have_State(TaskListItemFlags flags, VacancyTaskListSectionState expectedState)
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
        sut.SectionOneState.Should().Be(expectedState);
    }
    
    public static readonly IEnumerable<object[]> SectionOneFieldsTestCases =
        new List<object[]>
        {
            new object[] { (ProviderTaskListStateView v) => v.VacancyTitleEditable, TaskListItemFlags.NameOfEmployer },
            new object[] { (ProviderTaskListStateView v) => v.TrainingCourseEditable, TaskListItemFlags.AdvertTitle },
            new object[] { (ProviderTaskListStateView v) => v.ApprenticeshipSummaryEditable, TaskListItemFlags.TrainingCourse },
            new object[] { (ProviderTaskListStateView v) => v.WhatWillTheyDoAtWorkEditable, TaskListItemFlags.SummaryDescription },
            new object[] { (ProviderTaskListStateView v) => v.HowWillTheyTrainEditable, TaskListItemFlags.WhatWillTheyDoAtWork },
        };
    
    [TestCaseSource(nameof(SectionOneFieldsTestCases))]
    public void Section_One_Fields_Require_Dependent_Fields(Func<ProviderTaskListStateView, bool> fieldFunc, TaskListItemFlags flag)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
        vacancy.OwnerType = OwnerType.Provider;
        var states = Enum.GetValues<TaskListItemFlags>().ToDictionary(f => f, _ => true);
        states[flag] = false;
        
        // act
        var sut = new ProviderTaskListStateView(states, vacancy);
        bool actual = fieldFunc(sut);
        
        // assert
        actual.Should().BeFalse();
    }
}