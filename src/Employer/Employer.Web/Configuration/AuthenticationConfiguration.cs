namespace Esfa.Recruit.Employer.Web.Configuration
{
    public class AuthenticationConfiguration
    {
        public const int SessionTimeoutMinutes = 30;
        public string Authority { get; set; }
        public string MetaDataAddress { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }    
}
