using System;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Services.AlertViewModelServiceTests;

public abstract class AlertViewModelServiceTestBase
{

    protected DateTime? GetUserDismissedDate(string userLastDismissedDateString)
    {
        if (string.IsNullOrWhiteSpace(userLastDismissedDateString))
            return null;

        return DateTime.Parse(userLastDismissedDateString);
    }
}