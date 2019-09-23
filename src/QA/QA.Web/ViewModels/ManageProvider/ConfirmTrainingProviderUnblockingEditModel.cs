namespace Esfa.Recruit.QA.Web.ViewModels.ManageProvider
{
    public class ConfirmTrainingProviderUnblockingEditModel
    {
        public long Ukprn { get; set; }
        public string ProviderName { get; set; }
        public bool? RestoreAccess { get; set; }
        public bool CanRestoreAccess => RestoreAccess.HasValue && RestoreAccess.Value;
    }
}