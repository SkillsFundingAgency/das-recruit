namespace Esfa.Recruit.Employer.Web.Configuration.Routing;

public sealed class ManageApprenticeshipsRoutes
{
    public static string ManageApprenticeshipSiteAccountsHomeRoute => "/accounts/{0}/teams";
    public static string ManageApprenticeshipSiteRenameAccountRoute => "/accounts/{0}/rename";
    public static string ManageApprenticeshipSiteChangePasswordRoute => "/account/changepassword?clientId={0}&returnUrl={1}";
    public static string ManageApprenticeshipSiteChangeEmailAddressRoute => "/account/changeemail?clientId={0}&returnUrl={1}";
    public static string ManageApprenticeshipSiteAccountsFinanceRoute => "/accounts/{0}/finance";
    public static string ManageApprenticeshipSiteAccountsAgreementsRoute => "/accounts/{0}/agreements";
    public static string YourTrainingProviderPermissionRoute => "/accounts/{0}/providers";
    public static string EmployerRecruitmentApiRoute => "/accounts/{0}/recruitment/api";
}