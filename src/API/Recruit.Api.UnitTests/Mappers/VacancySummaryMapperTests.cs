using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using SFA.DAS.Recruit.Api.Configuration;
using SFA.DAS.Recruit.Api.Mappers;
using SFA.DAS.Recruit.Api.Services;
using Xunit;

namespace SFA.DAS.Recruit.Api.UnitTests.Mappers
{
    public class VacancySummaryMapperTests
    {
        private const string ValidEmployerAccountId = "MYJR4X";
        private readonly VacancySummaryMapper _sut;
        private readonly Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancySummary _vacancySummaryProjection;

        public VacancySummaryMapperTests()
        {
            var vacancySummaryFixture = new Fixture();
            _vacancySummaryProjection = vacancySummaryFixture
                                        .Build<Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancySummary>()
                                        .With(vsp => vsp.EmployerAccountId, ValidEmployerAccountId)
                                        .Create();

            var config = new RecruitConfiguration
            {
                FindAnApprenticeshipDetailPrefixUrl = "http://localhost:50218/apprenticeship/",
                EmployerRecruitAnApprenticeManageVacancyFormattedUrl = "https://employer-recruit/accounts/{0}/vacancies/{1}/manage/",
                ProviderRecruitAnApprenticeManageVacancyFormattedUrl = "https://provider-recruit/{0}/vacancies/{1}/manage/"
            };
            _sut = new VacancySummaryMapper(Options.Create(config));
        }

        [Fact]
        public void GivenEmployerVacancies_ShouldSetRaaManageVacancyUrlToEmployerManageVacancyUrl()
        {

            var vacancySummary = _sut.MapFromVacancySummaryProjection(_vacancySummaryProjection, isForProviderOwnedVacancies: false);

            vacancySummary.RaaManageVacancyUrl.StartsWith("https://employer-recruit/accounts").Should().BeTrue();
        }

        [Fact]
        public void GivenProviderVacancies_ShouldSetRaaManageVacancyUrlToProviderManageVacancyUrl()
        {
            var vacancySummary = _sut.MapFromVacancySummaryProjection(_vacancySummaryProjection, isForProviderOwnedVacancies: true);

            vacancySummary.RaaManageVacancyUrl.StartsWith("https://provider-recruit/").Should().BeTrue();
        }

        [Fact]
        public void GivenAEmployerVacancy_ShouldSetEmployerAccountAndVacancyIdInRaaManageVacancyUrl()
        {
            var vacancySummary = _sut.MapFromVacancySummaryProjection(_vacancySummaryProjection, isForProviderOwnedVacancies: false);
            var expectedEnd = $"accounts/{ValidEmployerAccountId}/vacancies/{_vacancySummaryProjection.Id.ToString()}/manage/";
            vacancySummary.RaaManageVacancyUrl.EndsWith(expectedEnd).Should().BeTrue();
        }

        [Fact]
        public void GivenAProviderVacancy_ShouldSetUkprnAndVacancyIdInRaaManageVacancyUrl()
        {
            var vacancySummary = _sut.MapFromVacancySummaryProjection(_vacancySummaryProjection, isForProviderOwnedVacancies: true);
            var expectedEnd = $"{_vacancySummaryProjection.Ukprn}/vacancies/{_vacancySummaryProjection.Id.ToString()}/manage/";
            vacancySummary.RaaManageVacancyUrl.EndsWith(expectedEnd).Should().BeTrue();
        }

        [Fact]
        public void GivenAVacancy_ShouldSetFaaDetailUrlWithVacancyReference()
        {
            var vacancySummary = _sut.MapFromVacancySummaryProjection(_vacancySummaryProjection, isForProviderOwnedVacancies: true);
            var expectedEnd = $"apprenticeship/{_vacancySummaryProjection.VacancyReference}";
            vacancySummary.FaaVacancyDetailUrl.EndsWith(expectedEnd).Should().BeTrue();
        }
    }
}