using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules;

public sealed class VacancyProfanityChecksRule : BaseProfanityChecksRule, IRule<Vacancy>
{
    private readonly IProfanityListProvider _profanityListProvider;
    public VacancyProfanityChecksRule(IProfanityListProvider profanityListProvider,
        ConsolidationOption consolidationOption = ConsolidationOption.NoConsolidation, 
        decimal weighting = 100.0m)
        : base(RuleId.ProfanityChecks, consolidationOption, weighting)
    {
        _profanityListProvider = profanityListProvider;
    }

    public async Task<RuleOutcome> EvaluateAsync(Vacancy subject)
    {
        var outcomeBuilder = RuleOutcomeDetailsBuilder.Create(RuleId);

        ProfanityList = await _profanityListProvider.GetProfanityListAsync();

        var outcomes = new List<RuleOutcome>();
        outcomes.AddRange(ProfanityCheckAsync(() => subject.Title));
        outcomes.AddRange(ProfanityCheckAsync(() => subject.ShortDescription));
        if (subject.EmployerLocation is not null)
        {
            outcomes.AddRange(ProfanityCheckAsync(() => subject.EmployerLocation.AddressLine1));
            outcomes.AddRange(ProfanityCheckAsync(() => subject.EmployerLocation.AddressLine2));
            outcomes.AddRange(ProfanityCheckAsync(() => subject.EmployerLocation.AddressLine3));
            outcomes.AddRange(ProfanityCheckAsync(() => subject.EmployerLocation.AddressLine4));
        }
        if (subject.EmployerLocations is { Count: > 0 })
        {
            outcomes.AddRange(ProfanityCheckAsync(() => subject.EmployerLocations.Select(x => x.Flatten()).ToDelimitedString(", "), "EmployerLocations"));
        }
        outcomes.AddRange(ProfanityCheckAsync(() => subject.EmployerLocationInformation));
        outcomes.AddRange(ProfanityCheckAsync(() => subject.Wage.WorkingWeekDescription));
        outcomes.AddRange(ProfanityCheckAsync(() => subject.Wage.WageAdditionalInformation));
        outcomes.AddRange(ProfanityCheckAsync(() => subject.Description));
        outcomes.AddRange(ProfanityCheckAsync(() => subject.TrainingDescription));
        outcomes.AddRange(ProfanityCheckAsync(() => subject.OutcomeDescription));
        outcomes.AddRange(ProfanityCheckAsync(() => subject.ThingsToConsider));
        outcomes.AddRange(ProfanityCheckAsync(() => subject.AdditionalQuestion1));
        outcomes.AddRange(ProfanityCheckAsync(() => subject.AdditionalQuestion2));
        if (subject.Skills != null)
            outcomes.AddRange(ProfanityCheckAsync(() => subject.Skills.ToDelimitedString(","), "Skills"));
        if (subject.Qualifications != null)
            outcomes.AddRange(ProfanityCheckAsync(() => subject.Qualifications.SelectMany(q => new[]{q.Grade, q.Subject}).ToDelimitedString(","), "Qualifications"));
        outcomes.AddRange(ProfanityCheckAsync(() => subject.EmployerDescription));

        if (subject.EmployerContact != null)
            outcomes.AddRange(ProfanityCheckAsync(() => subject.EmployerContact.Name));

        if (subject.ProviderContact != null)
            outcomes.AddRange(ProfanityCheckAsync(() => subject.ProviderContact.Name));

        if (subject.EmployerNameOption == EmployerNameOption.TradingName)
            outcomes.AddRange(ProfanityCheckAsync(() => subject.EmployerName));

        if (subject.EmployerNameOption == EmployerNameOption.Anonymous)
            outcomes.AddRange(ProfanityCheckAsync(() => subject.EmployerName));
            
        outcomes.AddRange(ProfanityCheckAsync(() => subject.ApplicationInstructions));

        var outcome = outcomeBuilder.Add(outcomes)
            .ComputeSum();

        return outcome;
    }
}