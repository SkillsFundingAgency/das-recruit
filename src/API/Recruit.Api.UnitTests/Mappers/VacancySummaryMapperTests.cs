using AutoFixture;
using Microsoft.Extensions.Options;
using SFA.DAS.Recruit.Api.Configuration;
using SFA.DAS.Recruit.Api.Mappers;

namespace SFA.DAS.Recruit.Api.UnitTests.Mappers
{
    public class VacancySummaryMapperTests
    {
        private const string ValidEmployerAccountId = "MYJR4X";
        private VacancySummaryMapper _sut;
        private Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancySummary _vacancySummaryProjection;

        [SetUp]
        public void Setup()
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

        [Test]
        public void GivenEmployerVacancies_ShouldSetRaaManageVacancyUrlToEmployerManageVacancyUrl()
        {

            var vacancySummary = _sut.MapFromVacancySummaryProjection(_vacancySummaryProjection, isForProviderOwnedVacancies: false);

            vacancySummary.RaaManageVacancyUrl.StartsWith("https://employer-recruit/accounts").Should().BeTrue();
        }

        [Test]
        public void GivenProviderVacancies_ShouldSetRaaManageVacancyUrlToProviderManageVacancyUrl()
        {
            var vacancySummary = _sut.MapFromVacancySummaryProjection(_vacancySummaryProjection, isForProviderOwnedVacancies: true);

            vacancySummary.RaaManageVacancyUrl.StartsWith("https://provider-recruit/").Should().BeTrue();
        }

        [Test]
        public void GivenAEmployerVacancy_ShouldSetEmployerAccountAndVacancyIdInRaaManageVacancyUrl()
        {
            var vacancySummary = _sut.MapFromVacancySummaryProjection(_vacancySummaryProjection, isForProviderOwnedVacancies: false);
            var expectedEnd = $"accounts/{ValidEmployerAccountId}/vacancies/{_vacancySummaryProjection.Id.ToString()}/manage/";
            vacancySummary.RaaManageVacancyUrl.EndsWith(expectedEnd).Should().BeTrue();
        }

        [Test]
        public void GivenAProviderVacancy_ShouldSetUkprnAndVacancyIdInRaaManageVacancyUrl()
        {
            var vacancySummary = _sut.MapFromVacancySummaryProjection(_vacancySummaryProjection, isForProviderOwnedVacancies: true);
            var expectedEnd = $"{_vacancySummaryProjection.Ukprn}/vacancies/{_vacancySummaryProjection.Id.ToString()}/manage/";
            vacancySummary.RaaManageVacancyUrl.EndsWith(expectedEnd).Should().BeTrue();
        }

        [Test]
        public void GivenAVacancy_ShouldSetFaaDetailUrlWithVacancyReference()
        {
            var vacancySummary = _sut.MapFromVacancySummaryProjection(_vacancySummaryProjection, isForProviderOwnedVacancies: true);
            var expectedEnd = $"apprenticeship/{_vacancySummaryProjection.VacancyReference}";
            vacancySummary.FaaVacancyDetailUrl.EndsWith(expectedEnd).Should().BeTrue();
        }
    }
}