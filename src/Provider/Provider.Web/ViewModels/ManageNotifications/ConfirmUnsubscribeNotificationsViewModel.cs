namespace Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications
{
    public class ConfirmUnsubscribeNotificationsViewModel
    {
        public ConfirmUnsubscribeNotificationsViewModel(long ukprn)
        {
            Ukprn = ukprn;
        }

        public long Ukprn { get; set; }

        public bool? ConfirmUnsubscribe { get; set; }
    }
}