using System;

namespace SFA.DAS.Recruit.Api.Models
{
    public class ApplicantSummary
    {
        public Guid ApplicantId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string ApplicationStatus { get; set; }
    }
}