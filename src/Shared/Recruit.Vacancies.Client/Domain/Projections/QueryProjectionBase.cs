using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Projections
{
    public abstract class QueryProjectionBase
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}