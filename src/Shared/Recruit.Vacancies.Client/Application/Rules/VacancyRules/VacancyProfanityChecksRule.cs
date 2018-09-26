using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.QA.Core.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules

{
    public sealed class VacancyProfanityChecksRule : BaseProfanityChecksRule, IRule<Vacancy>
    {
        public VacancyProfanityChecksRule(IProfanityListProvider profanityListProvider, ConsolidationOption consolidationOption = ConsolidationOption.NoConsolidation, decimal weighting = 100.0m)
            : base("ProfanityChecks", profanityListProvider, consolidationOption, weighting) { }

        public async Task<RuleOutcome> EvaluateAsync(Vacancy subject)
        {
            var outcomeBuilder = RuleOutcomeDetailsBuilder.Create(RuleId);

            var tasks = new List<Task<IEnumerable<RuleOutcome>>>
            {
                ProfanityCheckAsync(() => subject.Title, VacancyReview.FieldIdentifiers.Title),
                ProfanityCheckAsync(() => subject.ShortDescription, VacancyReview.FieldIdentifiers.ShortDescription),
                ProfanityCheckAsync(() => subject.EmployerLocation.AddressLine1, VacancyReview.FieldIdentifiers.EmployerAddress),
                ProfanityCheckAsync(() => subject.EmployerLocation.AddressLine2, VacancyReview.FieldIdentifiers.EmployerAddress),
                ProfanityCheckAsync(() => subject.EmployerLocation.AddressLine3, VacancyReview.FieldIdentifiers.EmployerAddress),
                ProfanityCheckAsync(() => subject.EmployerLocation.AddressLine4, VacancyReview.FieldIdentifiers.EmployerAddress),
                ProfanityCheckAsync(() => subject.Wage.WorkingWeekDescription, VacancyReview.FieldIdentifiers.WorkingWeek),
                ProfanityCheckAsync(() => subject.Wage.WageAdditionalInformation, VacancyReview.FieldIdentifiers.Wage),
                ProfanityCheckAsync(() => subject.Description, VacancyReview.FieldIdentifiers.VacancyDescription),
                ProfanityCheckAsync(() => subject.TrainingDescription, VacancyReview.FieldIdentifiers.TrainingDescription),
                ProfanityCheckAsync(() => subject.OutcomeDescription, VacancyReview.FieldIdentifiers.OutcomeDescription),
                ProfanityCheckAsync(() => subject.ThingsToConsider, VacancyReview.FieldIdentifiers.ThingsToConsider),
                ProfanityCheckAsync(() => subject.Skills.ToDelimitedString(","), VacancyReview.FieldIdentifiers.Skills),
                ProfanityCheckAsync(() => subject.Qualifications.SelectMany(q => $"{q.Grade}, {q.Subject}").ToDelimitedString(","), VacancyReview.FieldIdentifiers.Qualifications),
                ProfanityCheckAsync(() => subject.EmployerDescription, VacancyReview.FieldIdentifiers.EmployerDescription),
                ProfanityCheckAsync(() => subject.EmployerContactName, VacancyReview.FieldIdentifiers.EmployerContact),
                ProfanityCheckAsync(() => subject.ApplicationInstructions, VacancyReview.FieldIdentifiers.ApplicationInstructions)
            };

            await Task.WhenAll(tasks);

            var outcomes = tasks.SelectMany(t => t.Result);

            var outcome = outcomeBuilder.Add(outcomes)
                .ComputeSum();

            return outcome;
        }
    }
}
