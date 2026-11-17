using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Rules.VacancyRules;

public class TestProfanityListProvider : IProfanityListProvider
{
    public Task<IEnumerable<string>> GetProfanityListAsync()
    {
        return Task.FromResult<IEnumerable<string>>(["bother", "dang", "balderdash", "drat"]) ;
    }
}
    
public class VacancyProfanityChecksRuleTests
{
    [Test]
    public void WhenCreated_ItShouldReturnBasicInformationAboutTheRule()
    {
        var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider());
        rule.RuleId.Should().Be(RuleId.ProfanityChecks);
    }

    [TestCase("nothing", 0)]
    [TestCase("bother-bother, bother!", 3)]
    [TestCase("bother dang", 2)]
    [TestCase("drat", 1)]
    [TestCase("hydrate", 0)]
    [TestCase("dangleberry", 0)]
    [TestCase("bother", 100, 100)]
    [TestCase("bother dang", 100, 100)]
    public async Task WhenInvoked_ItShouldReturnTheExpectedScore(string phrase, int expectedScore, decimal weighting = 1.0m)
    {
        var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider(), ConsolidationOption.ConsolidateByField, weighting);
        var entity = TestVacancyBuilder.Create().SetTitle(phrase);

        var outcome = await rule.EvaluateAsync(entity);

        outcome.Score.Should().Be(expectedScore);
    }

    [Test]
    public async Task WhenInvoked_ItShouldReturnAnOverallUnconsolidatedNarrative()
    {
        var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider());
        string[] skills = ["Juggling", "Running", "dang"];
        var qualifications = new List<Qualification> { new Qualification{Grade = "dang grade", Subject = "subject"} };
        var entity = TestVacancyBuilder.Create()
            .SetTitle("bother-bother, bother!")
            .SetDescription("dang it and balderdash!!")
            .SetSkills(skills)
            .SetQualifications(qualifications);

        var outcome = await rule.EvaluateAsync(entity);

        outcome.Narrative.Should().Contain("Profanity 'bother' found 3 times in 'Title'");
        outcome.Narrative.Should().Contain("Profanity 'dang' found in 'Description'");
        outcome.Narrative.Should().Contain("Profanity 'balderdash' found in 'Description'");
        outcome.Narrative.Should().Contain("Profanity 'dang' found in 'Skills'");
        outcome.Narrative.Should().Contain("Profanity 'dang' found in 'Qualifications'");
    }

    [Test]
    public async Task WhenInvoked_ItShouldReturnAnOverallConsolidatedNarrative()
    {
        var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider(), ConsolidationOption.ConsolidateByField);
        string[] skills = ["Juggling", "Running", "dang"];
        var entity = TestVacancyBuilder.Create()
            .SetTitle("bother-bother, bother!")
            .SetDescription("dang it and balderdash!!")
            .SetSkills(skills);

        var outcome = await rule.EvaluateAsync(entity);

        outcome.Narrative.Should().Contain("3 profanities 'bother' found in 'Title'");
        outcome.Narrative.Should().Contain("2 profanities 'dang,balderdash' found in 'Description'");
        outcome.Narrative.Should().Contain("1 profanities 'dang' found in 'Skills'");
    }

    [Test]
    public async Task WhenInvoked_ItShouldIncludeDetailsOfEachFieldOutcome()
    {
        var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider());
        string[] skills = ["Juggling", "Running", "dang"];
        var entity = TestVacancyBuilder.Create()
            .SetTitle("bother-bother, bother!")
            .SetDescription("dang it and balderdash!!")
            .SetSkills(skills);

        var outcome = await rule.EvaluateAsync(entity);

        outcome.HasDetails.Should().BeTrue();
        outcome.Details.Should().HaveCount(20);
        outcome.Details.Should().AllSatisfy(x =>
        {
            x.Target.Should().NotBeNullOrEmpty();
            x.Narrative.Should().NotBeNullOrEmpty();
            x.RuleId.Should().Be(RuleId.ProfanityChecks);
        });
    }

    [Test]
    public async Task Profanities_Are_Detected_In_The_EmployerLocationInformation_Field()
    {
        // arrange
        var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider());
        var vacancy = new Vacancy
        {
            EmployerLocationInformation = "some text with bother in it",
            Wage = new Wage()
        };
            
        // act
        var outcome = await rule.EvaluateAsync(vacancy);
            
        // assert
        outcome.HasDetails.Should().BeTrue();
        outcome.Score.Should().Be(100);
        outcome.Narrative.Should().Contain("Profanity 'bother' found in 'EmployerLocationInformation'");
    }
        
    [Test]
    public async Task Profanities_Are_Detected_In_The_EmployerLocation_Field()
    {
        // arrange
        var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider());
        var vacancy = new Vacancy
        {
            EmployerLocation = new Address { AddressLine1 = "Address line 1", AddressLine2 = "Bother", AddressLine3 = "Address line 3" },
            Wage = new Wage()
        };
            
        // act
        var outcome = await rule.EvaluateAsync(vacancy);
            
        // assert
        outcome.HasDetails.Should().BeTrue();
        outcome.Score.Should().Be(100);
        outcome.Narrative.Should().Contain("Profanity 'bother' found in 'EmployerLocation.AddressLine2'");
    }
        
    [Test]
    public async Task Profanities_Are_Detected_In_The_EmployerLocations_Field()
    {
        // arrange
        var rule = new VacancyProfanityChecksRule(new TestProfanityListProvider());
        var vacancy = new Vacancy
        {
            EmployerLocations = [
                new Address { AddressLine1 = "Address line 1", AddressLine2 = "Address line 2", AddressLine3 = "Address line 3" },
                new Address { AddressLine1 = "Address line 1", AddressLine2 = "Bother", AddressLine3 = "Address line 3" },
            ],
            Wage = new Wage()
        };
            
        // act
        var outcome = await rule.EvaluateAsync(vacancy);
            
        // assert
        outcome.HasDetails.Should().BeTrue();
        outcome.Score.Should().Be(100);
        outcome.Narrative.Should().Contain("Profanity 'bother' found in 'EmployerLocations'");
    }
}