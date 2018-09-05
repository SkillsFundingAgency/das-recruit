namespace Esfa.Recruit.Qa.Web.Security
{
    public class AuthorizationConfiguration
    {
        public string ClaimType { get; set; }
        public string UserClaimValue { get; set; }
        public string TeamLeadClaimValue { get; set; }
    }
}