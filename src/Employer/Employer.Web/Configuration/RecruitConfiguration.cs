namespace Employer.Web.Configuration;

public class RecruitConfiguration(string employerAccountId)
{
    public string EmployerAccountId { get; set; } = employerAccountId;
}