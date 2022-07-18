namespace Esfa.Recruit.Provider.Web.Configuration
{
    public static class PolicyNames
    {
        public static string ProviderPolicyName => "ProviderPolicy";
        
        public static string HasContributorOrAbovePermission => "HasContributorOrAbovePermission";

        public static string HasContributorWithApprovalOrAbovePermission => "HasContributorWithApprovalOrAbovePermission";

        public static string HasAccountOwnerPermission => "HasAccountOwnerPermission";

        public static string IsTraineeshipWeb => nameof(IsTraineeshipWeb);
        
        public static string IsApprenticeshipWeb => nameof(IsApprenticeshipWeb);
    }
}
