using System;

namespace Esfa.Recruit.Storage.Client.Core.Entities
{
    public class Vacancy
    {
        public Guid Id { get; set; }
        
        public int VRN { get; set; }
        
        public string Title { get; set; }
    }
}
