namespace Employer.Web.Configuration
{
    public class RecruitConfiguration
    {
        public RecruitConfiguration(string employerAccountId)
        {
            EmployerAccountId = employerAccountId;
        }
        public string EmployerAccountId { get; set; }
        public bool UseGovSignIn { get; set; }
    }
}