using Esfa.Recruit.Vacancies.Client.Application.Rules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Rules.VacancyRules;

public class VacancyAnonymousCheckRuleTests
{
    [Fact]
    public async Task WhenInvoked_ForNonAnonymousEmployer()
    {
        var rule = new VacancyAnonymousCheckRule();

        var vacancy = new Vacancy 
        {
            EmployerNameOption = EmployerNameOption.RegisteredName
        };

        var outcome = await rule.EvaluateAsync(vacancy);

        outcome.RuleId.Should().Be(RuleId.VacancyAnonymous);
        outcome.Score.Should().Be(0);
    }

    [Fact]
    public async Task WhenInvoked_ForAnonymousEmployer()
    {
        var rule = new VacancyAnonymousCheckRule();

        var vacancy = new Vacancy {
            EmployerNameOption = EmployerNameOption.Anonymous
        };

        var outcome = await rule.EvaluateAsync(vacancy);

        outcome.RuleId.Should().Be(RuleId.VacancyAnonymous);
        outcome.Score.Should().Be(100);
    }
}