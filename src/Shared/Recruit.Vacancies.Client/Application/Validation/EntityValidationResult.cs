using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    public class EntityValidationResult
    {
        public EntityValidationResult()
        {
            Errors = new List<EntityValidationError>();
        }

        public bool HasErrors => Errors?.Count() > 0;

        public IList<EntityValidationError> Errors { get; set; }
    }
}