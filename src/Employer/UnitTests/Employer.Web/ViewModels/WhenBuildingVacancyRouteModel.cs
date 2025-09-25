using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels;

public class WhenBuildingVacancyRouteModel
{
    [Test, AutoData]
    public void Then_The_Properties_Are_Set_And_Dictionary_Built(Guid vacancyId, string employerAccountId)
    {
        var actual = new VacancyRouteModel
        {
            EmployerAccountId = employerAccountId,
            VacancyId = vacancyId
        };

        actual.RouteDictionary.Should().ContainKey("VacancyId");
        actual.RouteDictionary["VacancyId"].Should().Be(vacancyId.ToString());
        actual.RouteDictionary.Should().ContainKey("EmployerAccountId");
        actual.RouteDictionary["EmployerAccountId"].Should().Be(employerAccountId.ToUpper());
    }

    [Test, AutoData]
    public void And_No_VacancyId_Then_Not_Added_To_Dictionary(string employerAccountId)
    {
        var actual = new VacancyRouteModel
        {
            EmployerAccountId = employerAccountId
        };

        actual.RouteDictionary.Should().NotContainKey("VacancyId");
        actual.RouteDictionary.Should().ContainKey("EmployerAccountId");
        actual.RouteDictionary["EmployerAccountId"].Should().Be(employerAccountId.ToUpper());
    }
}