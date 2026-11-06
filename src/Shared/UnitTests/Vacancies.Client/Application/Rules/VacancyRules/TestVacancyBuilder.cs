using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Rules.VacancyRules;

internal static class TestVacancyBuilder
{
    internal static Vacancy Create()
    {
        return new Vacancy()
        {
            EmployerLocation = new Address(),
            EmployerContact = new ContactDetail(),
            Skills = new List<string>(),
            Wage = new Wage()
        };
    }

    internal static Vacancy SetVacancyReference(this Vacancy entity, long vacancyReference)
    {
        entity.VacancyReference = vacancyReference;

        return entity;
    }

    internal static Vacancy SetTitle(this Vacancy entity, string title)
    {
        entity.Title = title;
        return entity;
    }

    internal static Vacancy SetDescription(this Vacancy entity, string description)
    {
        entity.Description = description;
        return entity;
    }

    internal static Vacancy SetTrainingProgrammeId(this Vacancy entity, string programmeId)
    {
        entity.ProgrammeId = programmeId;
        return entity;
    }

    internal static Vacancy SetSkills(this Vacancy entity, IEnumerable<string> skills)
    {
        entity.Skills = skills.ToList();
        return entity;
    }
        
    internal static Vacancy SetQualifications(this Vacancy entity, IEnumerable<Qualification> qualifications)
    {
        entity.Qualifications = qualifications.ToList();
        return entity;
    }

    internal static Vacancy SetDetails(this Vacancy entity, string title, string description)
    {
        entity.Title = title;
        entity.Description = description;

        return entity;
    }
}