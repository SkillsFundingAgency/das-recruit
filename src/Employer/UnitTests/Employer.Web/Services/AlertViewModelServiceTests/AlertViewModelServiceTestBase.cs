
using System;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Services.AlertViewModelServiceTests
{
    public abstract class AlertViewModelServiceTestBase
    {

        protected DateTime? GetUserDismissedDate(string userLastDismissedDateString)
        {
            if (string.IsNullOrWhiteSpace(userLastDismissedDateString))
                return null;

            DateTime? userLastDismissedDate = DateTime.Parse(userLastDismissedDateString);
            userLastDismissedDate.Value.AddTicks(1);

            return userLastDismissedDate;
        }
    }
}
