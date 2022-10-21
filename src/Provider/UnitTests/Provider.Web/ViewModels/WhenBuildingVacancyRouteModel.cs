using System;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.RouteModel;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.ViewModels
{
    public class WhenBuildingVacancyRouteModel
    {
        [Test, AutoData]
        public void Then_The_Properties_Are_Set_And_Dictionary_Built(Guid vacancyId, int ukprn)
        {
            var actual = new VacancyRouteModel
            {
                Ukprn = ukprn,
                VacancyId = vacancyId
            };

            actual.RouteDictionary.Should().ContainKey("VacancyId");
            actual.RouteDictionary["VacancyId"].Should().Be(vacancyId.ToString());
            actual.RouteDictionary.Should().ContainKey("Ukprn");
            actual.RouteDictionary["Ukprn"].Should().Be(ukprn.ToString());
        }

        [Test, AutoData]
        public void Then_No_VacancyId_Then_Not_Added_To_Dictionary(int ukprn)
        {
            var actual = new VacancyRouteModel
            {
                Ukprn = ukprn
            };

            actual.RouteDictionary.Should().NotContainKey("VacancyId");
            actual.RouteDictionary.Should().ContainKey("Ukprn");
            actual.RouteDictionary["Ukprn"].Should().Be(ukprn.ToString());
        }
    }
}