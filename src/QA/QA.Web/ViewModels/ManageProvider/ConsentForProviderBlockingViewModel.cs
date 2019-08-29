using Humanizer;

namespace Esfa.Recruit.QA.Web.ViewModels.ManageProvider
{
    public class ConsentForProviderBlockingViewModel
    {
        public string Name { get; set; }
        public int PermissionCount { get; set; }
        public string Reason { get; set; }
        public bool HasConsent { get; set; }
        public bool ShowPermissionsMessage => PermissionCount > 0;
        public string EmployerText => "employer".ToQuantity(PermissionCount);
    }
}