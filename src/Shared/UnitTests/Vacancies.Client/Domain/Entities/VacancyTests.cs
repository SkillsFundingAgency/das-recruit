using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Domain.Entities;

public class VacancyTests
{
    [Test]
    public void CanSubmit_ShouldNotSubmitDeletedVacancy()
    {
        var vacancy = new Vacancy
        {
            Status = VacancyStatus.Draft,
            IsDeleted = true
        };

        vacancy.CanSubmit.Should().BeFalse();
    }

    [Test]
    public void CanSubmit_ShouldNotSubmitVacancyInUnexpectedStatus()
    {
        foreach (var status in Enum.GetValues<VacancyStatus>()
                     .Except([VacancyStatus.Draft, VacancyStatus.Referred, VacancyStatus.Rejected, VacancyStatus.Review
                     ]))
        {
            var vacancy = new Vacancy
            {
                Status = status,
                IsDeleted = false
            };

            vacancy.CanSubmit.Should().BeFalse();
        }
    }
}