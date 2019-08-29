namespace Esfa.Recruit.QA.Web.ViewModels.ManageProvider
{
    public class ConfirmTrainingProviderBlockingViewModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public long Ukprn { get; set; }
        public int PermissionCount { get; set; }
        public bool ShowPermissionsMessage => PermissionCount > 0;
        public bool HasMoreThanOnePermission => PermissionCount > 1;
    }
}