using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Rules.VacancyRules;

public class VacancyBannedPhraseChecksRuleTest
{
    private readonly IBannedPhrasesProvider _bannedPhrasesProvider;
    private readonly IEnumerable<string> _bannedPhrases = ["school", "schools", "driving license", "over 18"];
    public VacancyBannedPhraseChecksRuleTest()
    {
        var mockBannedPhraseProvider = new Mock<IBannedPhrasesProvider>();
        mockBannedPhraseProvider.Setup(b => b.GetBannedPhrasesAsync()).Returns(Task.FromResult(_bannedPhrases));
        _bannedPhrasesProvider = mockBannedPhraseProvider.Object;
    }

    [Test]
    public void ShouldIgnoreNonAlphaNumericCharacters()
    {
        var sut = new VacancyBannedPhraseChecksRule(_bannedPhrasesProvider);
        var vacancy = new Fixture()
            .Build<Vacancy>()
            .With(v => v.Title,
                @"Dolore school leavers �school"" leaver 5pm, shifts, may work evenings and weekends school") 
            .Create();

        var result = sut.EvaluateAsync(vacancy).Result;

        result.Details.First(d => d.Target == "Title");
        result.Score.Should().Be(100);
    }

    [TestCase("a driving license is required", 1)]
    [TestCase("driving - license required", 1)]
    [TestCase("*driving*license* is required", 1)]
    [TestCase("driving license", 100, 100)]
    [TestCase("driving license, driving license, driving license", 100, 100)]
    [TestCase("driving license over 18", 100, 100)]
    [TestCase("*driving \t \n license* is required", 1)]
    [TestCase("occasionally there may be some driving. licensees are required to like beer", 0)]
    [TestCase("a great mover, 18 moves at least!", 0)]
    [TestCase("school", 1)]
    public void WhenInvoked_ItShouldReturnTheExpectedScore(string phrase, int expectedScore,
        decimal weighting = 1.0m)
    {
        var rule = new VacancyBannedPhraseChecksRule(_bannedPhrasesProvider, ConsolidationOption.ConsolidateByField, weighting);
        var entity = TestVacancyBuilder.Create().SetDetails(phrase, string.Empty);

        var outcome = rule.EvaluateAsync(entity).Result;

        outcome.Score.Should().Be(expectedScore);
    }

    [Test]
    public void ShouldReturnAnOverallUnconsolidatedNarrative()
    {
        var rule = new VacancyBannedPhraseChecksRule(_bannedPhrasesProvider);
        string[] skills = ["Juggling", "Running", "driving license"];
        var qualifications = new List<Qualification> { new Qualification{Grade = "over 18 grade", Subject = "subject"} };
        var entity = TestVacancyBuilder.Create()
            .SetDetails("driving - license required", "must be over 18 years of age and hold a clean driving license, yes a driving license!")
            .SetSkills(skills)
            .SetQualifications(qualifications);

        var outcome = rule.EvaluateAsync(entity).Result;

        outcome.Narrative.Should().Contain("Banned phrase 'driving license' found in 'Title'");
        outcome.Narrative.Should().Contain("Banned phrase 'driving license' found 2 times in 'Description'");
        outcome.Narrative.Should().Contain("Banned phrase 'driving license' found in 'Skills'");
        outcome.Narrative.Should().Contain("Banned phrase 'over 18' found in 'Description'");
        outcome.Narrative.Should().Contain("Banned phrase 'over 18' found in 'Qualifications'");
    }

    [Test]
    public void ShouldReturnAnOverallConsolidatedNarrative()
    {
        var rule = new VacancyBannedPhraseChecksRule(_bannedPhrasesProvider, ConsolidationOption.ConsolidateByField);
        string[] skills = ["Juggling", "Running", "driving license"];
        var entity = TestVacancyBuilder.Create()
            .SetDetails("driving - license required", "must be over 18 years of age and hold a clean driving license, yes a driving license!")
            .SetSkills(skills);

        var outcome = rule.EvaluateAsync(entity).Result;

        outcome.Narrative.Should().Contain("1 banned phrases 'driving license' found in 'Title'");
        outcome.Narrative.Should().Contain("3 banned phrases 'driving license,over 18' found in 'Description'");
        outcome.Narrative.Should().Contain("1 banned phrases 'driving license' found in 'Skills'");
    }

    [Test]
    public void WhenInvoked_ItShouldIncludeDetailsOfEachFieldOutcome()
    {
        var rule = new VacancyBannedPhraseChecksRule(_bannedPhrasesProvider);
        string[] skills = ["Juggling", "Running", "driving license"];
        var entity = TestVacancyBuilder.Create()
            .SetDetails("an apprenticeship", "must be over 18 years of age and hold a clean driving license")
            .SetSkills(skills);

        var outcome = rule.EvaluateAsync(entity).Result;

        outcome.HasDetails.Should().BeTrue();
        outcome.Details.Should().AllSatisfy(x =>
        {
            x.Target.Should().NotBeEmpty();
            x.Narrative.Should().NotBeEmpty();
        });
    }
        
    [Test]
    public async Task Banned_Phrases_Are_Detected_In_The_EmployerLocationInformation_Field()
    {
        // arrange
        var rule = new VacancyBannedPhraseChecksRule(_bannedPhrasesProvider);
        var vacancy = new Vacancy
        {
            EmployerLocationInformation = "some text with Driving license in it",
            Wage = new Wage()
        };
            
        // act
        var outcome = await rule.EvaluateAsync(vacancy);
            
        // assert
        outcome.HasDetails.Should().BeTrue();
        outcome.Score.Should().Be(100);
        outcome.Narrative.Should().Contain("Banned phrase 'driving license' found in 'EmployerLocationInformation'");
    }

    [Test]
    public async Task Banned_Phrases_Are_Detected_In_The_EmployerLocation_Field()
    {
        // arrange
        var rule = new VacancyBannedPhraseChecksRule(_bannedPhrasesProvider);
        var vacancy = new Vacancy
        {
            EmployerLocation = new Address { AddressLine1 = "Address line 1", AddressLine2 = "Driving License", AddressLine3 = "Address line 3" }, 
            Wage = new Wage()
        };
            
        // act
        var outcome = await rule.EvaluateAsync(vacancy);
            
        // assert
        outcome.HasDetails.Should().BeTrue();
        outcome.Score.Should().Be(100);
        outcome.Narrative.Should().Contain("Banned phrase 'driving license' found in 'EmployerLocation.AddressLine2'");
    }
        
    [Test]
    public async Task Banned_Phrases_Are_Detected_In_The_EmployerLocations_Field()
    {
        // arrange
        var rule = new VacancyBannedPhraseChecksRule(_bannedPhrasesProvider);
        var vacancy = new Vacancy
        {
            EmployerLocations = [
                new Address { AddressLine1 = "Address line 1", AddressLine2 = "Address line 2", AddressLine3 = "Address line 3" },
                new Address { AddressLine1 = "Address line 1", AddressLine2 = "Driving License", AddressLine3 = "Address line 3" },
            ],
            Wage = new Wage()
        };
            
        // act
        var outcome = await rule.EvaluateAsync(vacancy);
            
        // assert
        outcome.HasDetails.Should().BeTrue();
        outcome.Score.Should().Be(100);
        outcome.Narrative.Should().Contain("Banned phrase 'driving license' found in 'EmployerLocations'");
    }
}