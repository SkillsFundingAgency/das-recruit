namespace Esfa.Recruit.Provider.Web.Configuration
{
    public class ProviderRecruitClaims
    {
        public static string IdamsUserDisplayNameClaimTypeIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/displayname";
        public static string DfEUserDisplayNameClaimTypeIdentifier = "http://schemas.portal.com/displayname";
        public static string IdamsUserNameClaimTypeIdentifier  ="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        public static string DfEUserNameClaimTypeIdentifier  ="http://schemas.portal.com/name";
        public static string DfEUserIdClaimTypeIdentifier  ="sub";
        public static string IdamsUserServiceTypeClaimTypeIdentifier  = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/service";
        public static string DfEUserServiceTypeClaimTypeIdentifier  = "http://schemas.portal.com/service";
        public static string IdamsUserUkprnClaimsTypeIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/ukprn";
        public static string DfEUkprnClaimsTypeIdentifier = "http://schemas.portal.com/ukprn";
        public static string IdamsUserEmailClaimTypeIdentifier  = "http://schemas.portal.com/mail";
    }
}