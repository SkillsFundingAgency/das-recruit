namespace Esfa.Recruit.QA.Web.ViewModels.ManageProvider
{
    public class ConsentForProviderBlockingViewModel
    {
        public string Name { get; set; }
        public int PermissionCount { get; set; }
        public string Reason { get; set; }
        public bool HasConsent { get; set; }
        public bool ShowPermissionsMessage => PermissionCount > 0;
        public bool HasMoreThanOnePermission => PermissionCount > 1;

    }
}