using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Jobs.Jobs;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Jobs.UnitTests.Jobs
{
    public class TransferVacanciesFromEmployerReviewToQAReviewJobTests
    {
        private const string UserEmail = "test@test.com";
        private const string UserName = "John Smith";
        private readonly Mock<IVacancyQuery> _mockVacancyQuery;
        private readonly Mock<IMessaging> _mockMessaging;
        private readonly TransferVacanciesFromEmployerReviewToQAReviewJob _sut;

        public TransferVacanciesFromEmployerReviewToQAReviewJobTests()
        {
            _mockVacancyQuery = new Mock<IVacancyQuery>();
            _mockMessaging = new Mock<IMessaging>();
            _sut = new TransferVacanciesFromEmployerReviewToQAReviewJob(_mockVacancyQuery.Object, _mockMessaging.Object);
        }

        [Fact]
        public async Task GivenProviderHasVacanciesInReview_ThenShouldProcessVacancyToTransfer()
        {
            var userRef = Guid.NewGuid();
            const long Ukprn = 1;
            const string AccountLegalEntityPublicHashedId = "2";

            _mockVacancyQuery.Setup(x => x.GetProviderOwnedVacanciesInReviewAsync(Ukprn, AccountLegalEntityPublicHashedId))
                .ReturnsAsync(
                    new List<Vacancy>
                    {
                        new Vacancy
                        {
                            Id = Guid.NewGuid()
                        },
                        new Vacancy
                        {
                            Id= Guid.NewGuid()
                        }
                    });

            await _sut.Run(Ukprn, AccountLegalEntityPublicHashedId, userRef, UserEmail, UserName);

            _mockMessaging.Verify(x => x.SendCommandAsync(It.IsAny<TransferEmployerReviewToQAReviewCommand>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GivenProviderHasNoVacanciesInReview_ThenShouldNotProcessVacancyToTransfer()
        {
            var userRef = Guid.NewGuid();
            const long Ukprn = 1;
            const string AccountLegalEntityPublicHashedId = "2";

            _mockVacancyQuery.Setup(x => x.GetProviderOwnedVacanciesInReviewAsync(Ukprn, AccountLegalEntityPublicHashedId))
                .ReturnsAsync(Enumerable.Empty<Vacancy>());

            await _sut.Run(Ukprn, AccountLegalEntityPublicHashedId, userRef, UserEmail, UserName);

            _mockMessaging.Verify(x => x.SendCommandAsync(It.IsAny<TransferEmployerReviewToQAReviewCommand>()), Times.Never);
        }
    }
}