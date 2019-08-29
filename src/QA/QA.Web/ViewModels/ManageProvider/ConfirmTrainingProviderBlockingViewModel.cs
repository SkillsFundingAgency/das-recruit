using Humanizer;
namespace Esfa.Recruit.QA.Web.ViewModels.ManageProvider
{
    public class ConfirmTrainingProviderBlockingViewModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public long Ukprn { get; set; }
        public int PermissionCount { get; set; }
        public bool ShowPermissionsMessage => PermissionCount > 0;
        public string EmployerText => "employer".ToQuantity(PermissionCount) + (PermissionCount > 1 ? " have" : " has");
    }
}