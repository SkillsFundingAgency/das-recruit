using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class WhenConstructingGetBankHolidaysUrl
{
    [Test]
    public void Then_It_Is_Correctly_Constructed()
    {
        var actual = new GetBankHolidaysRequest();
            
        actual.GetUrl.Should().Be("bankholidays");
    }
}