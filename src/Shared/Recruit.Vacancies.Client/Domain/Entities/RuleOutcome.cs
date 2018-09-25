using System;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class RuleOutcome
    {
        public const string NoSpecificTarget = "";

        public RuleOutcome(string ruleId, int score, string narrative, string target = NoSpecificTarget, IEnumerable<RuleOutcome> details = null)
        {
            RuleId = ruleId;
            Score = Math.Min(score, 100);
            Narrative = narrative;
            Target = target;

            if (details != null)
                Details = details;
        }

        public IEnumerable<RuleOutcome> Details { get; set; }
        public bool HasDetails => Details.Any();
        public string RuleId { get; set; }
        public int Score { get; set;  }
        public string Narrative { get; set;  }

        /// <summary>
        /// Field or reference that this outcome relates to
        /// </summary>
        public string Target { get; set;  }

        public override string ToString()
        {
            return $"Rule {RuleId} score {Score} ({(HasDetails ? $"{Details.Count()} details" : $"'{Narrative}'")})";
        }
    }
}
