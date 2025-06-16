using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Domain.TaskListValidatorTests;

public class WhenValidatingEmployerTaskList
{
    [Test, MoqAutoData]
    public async Task Then_The_Task_List_Is_Valid(Vacancy vacancy)
    {
        // arrange
        var sut = new TaskListValidator();
        vacancy.OwnerType = OwnerType.Employer;
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
    
        // act
        bool result = await sut.IsCompleteAsync(vacancy, EmployerTaskListSectionFlags.All);
    
        // assert
        result.Should().Be(true);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Task_List_Is_Valid_With_A_Provider_Generated_Vacancy(Vacancy vacancy)
    {
        // arrange
        var sut = new TaskListValidator();
        vacancy.OwnerType = OwnerType.Provider;
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
    
        // act
        bool result = await sut.IsCompleteAsync(vacancy, EmployerTaskListSectionFlags.All);
    
        // assert
        result.Should().Be(true);
    }
    
    [Test]
    public async Task Individual_VacancyTaskListItemFlags_Return_True([Values] TaskListItemFlags flag)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.HasOptedToAddQualifications = true;
        vacancy.HasSubmittedAdditionalQuestions = true;
        var sut = new TaskListValidator();
    
        // act
        bool isItemValid = await sut.IsCompleteAsync(vacancy, flag);
    
        // assert
        isItemValid.Should().BeTrue();
    }
    
    public static readonly IEnumerable<object[]> SectionOneTestCases =
        new List<object[]>
        {
            new object[] { (Vacancy v) => { v.Title = null; } },
            new object[] { (Vacancy v) => { v.AccountLegalEntityPublicHashedId = null; } },
            new object[] { (Vacancy v) => { v.ProgrammeId = null; } },
            new object[] { (Vacancy v) => { v.TrainingProvider = null; } },
            new object[] { (Vacancy v) => { v.TrainingProvider.Ukprn = null; } },
            new object[] { (Vacancy v) => { v.ShortDescription = string.Empty; } },
            new object[] { (Vacancy v) => { v.Description = null; } },
        };
    
    [TestCaseSource(nameof(SectionOneTestCases))]
    public async Task Section_One_Requires_Field(Action<Vacancy> setupAction)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
        vacancy.OwnerType = OwnerType.Employer;
        setupAction(vacancy);
        
        var sut = new TaskListValidator();
        
        // act
        bool result = await sut.IsCompleteAsync(vacancy, EmployerTaskListSectionFlags.One);
        
        // assert
        result.Should().BeFalse();
    }
    
    
    public static readonly IEnumerable<object[]> SectionTwoTestCases =
        new List<object[]>
        {
            new object[] { (Vacancy v) => { v.ClosingDate = null; } },
            new object[] { (Vacancy v) => { v.StartDate = null; } },
            new object[] { (Vacancy v) => { v.Wage = null; } },
            new object[] { (Vacancy v) => { v.Wage.Duration = null; } },
            new object[] { (Vacancy v) => { v.Wage.WorkingWeekDescription = null; } },
            new object[] { (Vacancy v) => { v.Wage.WeeklyHours = null; } },
            new object[] { (Vacancy v) => { v.Wage.WageType = null; } },
            new object[] { (Vacancy v) => { v.NumberOfPositions = null; } },
            new object[] {
                (Vacancy v) =>
                {
                    v.EmployerLocationOption = null;
                    v.EmployerLocation = null;
                } },
            new object[] {
                (Vacancy v) =>
                {
                    v.EmployerLocationOption = AvailableWhere.AcrossEngland;
                    v.EmployerLocationInformation = null;
                } },
            new object[] {
                (Vacancy v) =>
                {
                    v.EmployerLocationOption = AvailableWhere.MultipleLocations;
                    v.EmployerLocations = null;
                } },
            new object[] {
                (Vacancy v) =>
                {
                    v.EmployerLocationOption = AvailableWhere.OneLocation;
                    v.EmployerLocations = null;
                } },
        };
    
    [TestCaseSource(nameof(SectionTwoTestCases))]
    public async Task Section_Two_Requires_Field(Action<Vacancy> setupAction)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
        vacancy.OwnerType = OwnerType.Employer;
        setupAction(vacancy);
        
        var sut = new TaskListValidator();
        
        // act
        bool result = await sut.IsCompleteAsync(vacancy, EmployerTaskListSectionFlags.Two);
        
        // assert
        result.Should().BeFalse();
    }
    
    public static readonly IEnumerable<object[]> SectionThreeTestCases =
        new List<object[]>
        {
            new object[] { (Vacancy v) => { v.Skills = null; } },
            new object[] { (Vacancy v) => { v.Skills = []; } },
            new object[] { (Vacancy v) => { v.HasOptedToAddQualifications = null; } },
            new object[] {
                (Vacancy v) =>
                {
                    v.HasOptedToAddQualifications = true;
                    v.Qualifications = null;
                } },
            new object[] {
                (Vacancy v) =>
                {
                    v.HasOptedToAddQualifications = true;
                    v.Qualifications = [];
                } },
            new object[] { (Vacancy v) => { v.OutcomeDescription = null; } },
        };
    
    [TestCaseSource(nameof(SectionThreeTestCases))]
    public async Task Section_Three_Requires_Field(Action<Vacancy> setupAction)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
        vacancy.OwnerType = OwnerType.Employer;
        setupAction(vacancy);
        
        var sut = new TaskListValidator();
        
        // act
        bool result = await sut.IsCompleteAsync(vacancy, EmployerTaskListSectionFlags.Three);
        
        // assert
        result.Should().BeFalse();
    }
    
    public static readonly IEnumerable<object[]> SectionFourTestCases =
        new List<object[]>
        {
            new object[] { (Vacancy v) => { v.EmployerNameOption = null; } },
            new object[] { (Vacancy v) => { v.EmployerDescription = null; } },
            new object[] { (Vacancy v) => { v.ApplicationMethod = null; } },
        };
    
    [TestCaseSource(nameof(SectionFourTestCases))]
    public async Task Section_Four_Requires_Field(Action<Vacancy> setupAction)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
        vacancy.OwnerType = OwnerType.Employer;
        setupAction(vacancy);
        
        var sut = new TaskListValidator();
        
        // act
        bool result = await sut.IsCompleteAsync(vacancy, EmployerTaskListSectionFlags.Four);
        
        // assert
        result.Should().BeFalse();
    }
    
    public static readonly IEnumerable<object[]> SectionFiveTestCases =
        new List<object[]>
        {
            new object[] { (Vacancy v) => { v.HasSubmittedAdditionalQuestions = false; } },
        };
    
    [TestCaseSource(nameof(SectionFiveTestCases))]
    public async Task Section_Five_Requires_Field(Action<Vacancy> setupAction)
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
        vacancy.OwnerType = OwnerType.Employer;
        setupAction(vacancy);
        
        var sut = new TaskListValidator();
        
        // act
        bool result = await sut.IsCompleteAsync(vacancy, EmployerTaskListSectionFlags.Five);
        
        // assert
        result.Should().BeFalse();
    }
    
    [Test]
    public async Task Section_Five_Does_Not_Require_Fields_When_Application_Method_Is_External_Website()
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.HasSubmittedAdditionalQuestions = false;
        vacancy.AdditionalQuestion1 = null;
        vacancy.AdditionalQuestion2 = null;
        vacancy.ApplicationMethod = ApplicationMethod.ThroughExternalApplicationSite;
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Standard;
        vacancy.OwnerType = OwnerType.Employer;
        
        var sut = new TaskListValidator();
        
        // act
        bool result = await sut.IsCompleteAsync(vacancy, EmployerTaskListSectionFlags.Five);
        
        // assert
        result.Should().BeTrue();
    }
    
    [Test]
    public async Task Section_Three_For_Foundation_Apprenticeship_Does_Not_Require_Skills_Or_Qualifications()
    {
        // arrange
        var vacancy = new Fixture().Create<Vacancy>();
        vacancy.ApprenticeshipType = ApprenticeshipTypes.Foundation;
        vacancy.OwnerType = OwnerType.Employer;
        vacancy.Skills = null;
        vacancy.Qualifications = null;
        vacancy.HasOptedToAddQualifications = null;
        
        var sut = new TaskListValidator();
        
        // act
        bool result = await sut.IsCompleteAsync(vacancy, EmployerTaskListSectionFlags.Three);
        
        // assert
        result.Should().BeTrue();
    }
    
    [Test]
    [MoqInlineAutoData(EmployerTaskListSectionFlags.One)]
    [MoqInlineAutoData(EmployerTaskListSectionFlags.Two)]
    [MoqInlineAutoData(EmployerTaskListSectionFlags.Three)]
    [MoqInlineAutoData(EmployerTaskListSectionFlags.Four)]
    [MoqInlineAutoData(EmployerTaskListSectionFlags.Five)]
    [MoqInlineAutoData(EmployerTaskListSectionFlags.All)]
    public async Task GetItemStatesAsync_Returns_Section_Specific_Results(EmployerTaskListSectionFlags flags, Vacancy vacancy)
    {
        // arrange
        var sut = new TaskListValidator();
        
        // act
        var results = await sut.GetItemStatesAsync(vacancy, (TaskListItemFlags)flags);
    
        // assert
        results.Should().HaveCount(BitOperations.PopCount((ulong)flags));
        results.Should().AllSatisfy(x =>
        {
            ulong lh = (ulong)x.Key;
            ulong rh = (ulong)flags;
    
            (lh & rh).Should().Be(lh);
        });
    }
    
    [Test]
    [MoqInlineAutoData(TaskListItemFlags.AdvertTitle)]
    [MoqInlineAutoData(TaskListItemFlags.FutureProspects)]
    [MoqInlineAutoData(TaskListItemFlags.Skills | TaskListItemFlags.Qualifications)]
    public async Task GetItemStatesAsync_Returns_Flag_Specific_Results(TaskListItemFlags flags, Vacancy vacancy)
    {
        // arrange
        var sut = new TaskListValidator();
        
        // act
        var results = await sut.GetItemStatesAsync(vacancy, flags);
    
        // assert
        results.Should().HaveCount(BitOperations.PopCount((ulong)flags));
        results.Should().AllSatisfy(x =>
        {
            ulong lh = (ulong)x.Key;
            ulong rh = (ulong)flags;
    
            (lh & rh).Should().Be(lh);
            x.Value.Should().BeTrue();
        });
    }
    
    [Test]
    public async Task GetItemStatesAsync_Validates_Items_As_False()
    {
        // arrange
        var sut = new TaskListValidator();
        var vacancy = new Vacancy();
        
        // act
        var results = await sut.GetItemStatesAsync(vacancy, EmployerTaskListSectionFlags.All);
    
        // assert
        results.Should().HaveCount(BitOperations.PopCount((ulong)EmployerTaskListSectionFlags.All));
        results.Count(x => x.Value is false).Should().Be(18);
    }
}