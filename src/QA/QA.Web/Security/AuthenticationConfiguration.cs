namespace Esfa.Recruit.Qa.Web.Security
{
    public class AuthenticationConfiguration
    {
        public const int SessionTimeoutMinutes = 30;
        public string Wtrealm { get; set; }
        public string MetaDataAddress { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether DfeSignIn is allowed.
        /// </summary>
        public bool UseDfeSignIn { get; set; } = false;
    }   
}