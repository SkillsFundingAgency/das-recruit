using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string IdamsUserId { get; set; } 
        public UserType UserType { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastSignedInDate { get; set; }
    }
}
