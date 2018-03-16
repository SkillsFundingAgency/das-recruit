using System;
using System.Collections.Generic;
using System.Text;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class LegalEntity
    {
        public long LegalEntityId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public Address Address { get; set; }
    }
}
