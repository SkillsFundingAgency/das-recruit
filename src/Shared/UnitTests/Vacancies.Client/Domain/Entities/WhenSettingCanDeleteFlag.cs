using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Domain.Entities;

public class WhenSettingCanDeleteFlag
{
    [TestCase(VacancyStatus.Draft)]
    [TestCase(VacancyStatus.Referred)]
    [TestCase(VacancyStatus.Rejected)]
    public void Then_Can_Delete_If_Correct_Status_And_Not_Deleted(VacancyStatus vacancyStatus)
    {
        var vacancy = new Vacancy
        {
            Status = vacancyStatus,
            IsDeleted = false
        };
            
        vacancy.CanDelete.Should().BeTrue();
    }

    [Test]
    public void Then_Can_Delete_If_Submitted_But_Closing_Date_Has_Lapsed()
    {
        var vacancy = new Vacancy
        {
            Status = VacancyStatus.Submitted,
            ClosingDate = DateTime.UtcNow.AddDays(-1),
            IsDeleted = false
        };
            
        vacancy.CanDelete.Should().BeTrue();
    }

    [Test]
    public void Then_Can_Not_Delete_If_Submitted_But_Not_Passed_Closing_Date()
    {
        var vacancy = new Vacancy
        {
            Status = VacancyStatus.Submitted,
            ClosingDate = DateTime.UtcNow.AddDays(1),
            IsDeleted = false
        };
            
        vacancy.CanDelete.Should().BeFalse();
    }
        
    [TestCase(VacancyStatus.Review)]
    [TestCase(VacancyStatus.Live)]
    [TestCase(VacancyStatus.Closed)]
    [TestCase(VacancyStatus.Approved)]
    public void Then_Can_Not_Delete_If_Incorrect_Status_And_Not_Deleted(VacancyStatus vacancyStatus)
    {
        var vacancy = new Vacancy
        {
            Status = vacancyStatus,
            IsDeleted = false
        };
            
        vacancy.CanDelete.Should().BeFalse();
    }

    [TestCase(VacancyStatus.Draft)]
    [TestCase(VacancyStatus.Referred)]
    [TestCase(VacancyStatus.Rejected)]
    [TestCase(VacancyStatus.Submitted)]
    public void Then_Can_Not_Delete_If_Deleted(VacancyStatus vacancyStatus)
    {
        var vacancy = new Vacancy
        {
            Status = vacancyStatus,
            IsDeleted = true,
            ClosingDate = DateTime.UtcNow.AddDays(-1)
        };
            
        vacancy.CanDelete.Should().BeFalse();
    }
}