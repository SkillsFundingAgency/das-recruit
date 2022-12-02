namespace Esfa.Recruit.Qa.Web.Security
{
    public class AuthenticationConfiguration
    {
        public const int SessionTimeoutMinutes = 30;
        public string Wtrealm { get; set; }
        public string MetaDataAddress { get; set; }
        public bool UseDfeSignIn { get; set; }
    }   
}