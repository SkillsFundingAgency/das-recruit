using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class ReviewSummaryService : IReviewSummaryService
    {
        private readonly IEmployerVacancyClient _client;
        public ReviewSummaryService(IEmployerVacancyClient client)
        {
            _client = client;
        }

        public async Task<ReviewSummaryViewModel> GetReviewSummaryViewModel(long vacancyReference, IEnumerable<ReviewFieldIndicatorViewModel> reviewFieldIndicatorsForPage)
        {
            ReviewSummaryViewModel vm;
            var review = await _client.GetCurrentReferredVacancyReviewAsync(vacancyReference);

            var automatedResult = new RuleSetOutcome();

            if (review != null)
            {
                var fieldIndicators = ReviewFieldIndicatorMapper.MapFromFieldIndicators(reviewFieldIndicatorsForPage, review.ManualQaFieldIndicators, automatedResult).ToList();

                vm = new ReviewSummaryViewModel
                {
                    CanDisplayReviewHeader = true,
                    ReviewerComments = review.ManualQaComment,
                    FieldIndicators = fieldIndicators
                };
            }
            else
            {
                vm = new ReviewSummaryViewModel { CanDisplayReviewHeader = false };
            }

            return vm;
        }
    }
    public enum RuleSetDecision
    {
        Unknown = 0,
        Refer,
        Approve,
        Indeterminate
    }

    public sealed class RuleSetOutcome
    {
        public RuleSetOutcome()
        {
            Decision = RuleSetDecision.Refer;
            TotalScore = 90;
            RuleOutcomes = new List<RuleOutcome>
            {
                new RuleOutcome("ProfanityRule", "Some Narrative", new List<RuleOutcome>{ new RuleOutcome("TrainingDescription", "The Training Desc is well bad", null), new RuleOutcome("VacancyDescription", "The Vacancy Desc is well bad", null)  }),
                new RuleOutcome("AddressCheckRule", "The Address is well bad", new List<RuleOutcome> { new RuleOutcome("EmployerAddress", "This is a bad address", null) })
            };
        }

        public RuleSetDecision Decision { get; private set; }
        public int TotalScore { get; set; }
        public IEnumerable<RuleOutcome> RuleOutcomes { get; set; }
    }

    public sealed class RuleOutcome
    {
        public RuleOutcome(string target, string narrative, IEnumerable<RuleOutcome> childOutcomes)
        {
            Target = target;
            Narrative = narrative;
            Score = 80;
            Details = childOutcomes ?? new List<RuleOutcome>();
        }

        public IEnumerable<RuleOutcome> Details { get; set; }
        public bool HasDetails => Details.Any();
        public string RuleId { get; }
        public int Score { get; }
        public string Narrative { get; }
        public string Target { get; }

    }
}
