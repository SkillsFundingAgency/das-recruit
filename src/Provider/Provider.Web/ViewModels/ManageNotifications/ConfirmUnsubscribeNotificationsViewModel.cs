namespace Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications;

public class ConfirmUnsubscribeNotificationsViewModel(long ukprn)
{
    public long Ukprn { get; set; } = ukprn;

    public bool? ConfirmUnsubscribe { get; set; }
}