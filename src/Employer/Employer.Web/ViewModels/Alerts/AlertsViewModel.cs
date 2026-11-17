namespace Esfa.Recruit.Employer.Web.ViewModels.Alerts;

public class AlertsViewModel(
    EmployerTransferredVacanciesAlertViewModel employerRevokedTransferredVacanciesAlert,
    EmployerTransferredVacanciesAlertViewModel blockedProviderTransferredVacanciesAlert,
    BlockedProviderAlertViewModel blockedProviderAlert,
    WithdrawnVacanciesAlertViewModel withdrawnByQaVacanciesAlert)
{
    public EmployerTransferredVacanciesAlertViewModel EmployerRevokedTransferredVacanciesAlert { get; internal set; } = employerRevokedTransferredVacanciesAlert;
    public EmployerTransferredVacanciesAlertViewModel BlockedProviderTransferredVacanciesAlert { get; internal set; } = blockedProviderTransferredVacanciesAlert;
    public BlockedProviderAlertViewModel BlockedProviderAlert { get; internal set; } = blockedProviderAlert;
    public WithdrawnVacanciesAlertViewModel WithdrawnByQaVacanciesAlert { get; internal set; } = withdrawnByQaVacanciesAlert;

    public bool ShowEmployerRevokedTransferredVacanciesAlert => EmployerRevokedTransferredVacanciesAlert is
    {
        TransferredVacanciesCount: > 0
    };
    public bool ShowBlockedProviderTransferredVacanciesAlert => BlockedProviderTransferredVacanciesAlert is
    {
        TransferredVacanciesCount: > 0
    };
    public bool ShowBlockedProviderAlert => BlockedProviderAlert is
    {
        HasMultipleBlockedProviders: true
    };
    public bool ShowWithdrawnByQaVacanciesAlert => WithdrawnByQaVacanciesAlert is
    {
        ClosedVacanciesCount: > 0
    };
}