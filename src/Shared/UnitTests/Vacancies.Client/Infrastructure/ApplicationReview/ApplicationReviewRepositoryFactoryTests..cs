using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.ApplicationReview
{
    [TestFixture]
    public class ApplicationReviewRepositoryFactoryTests
    {
        [Test]
        public void GetRepository_WithSqlType_ReturnsApplicationReviewService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddTransient<ApplicationReviewService, FakeApplicationReviewService>()
                .AddTransient<MongoDbApplicationReviewRepository, FakeMongoDbApplicationReviewRepository>()
                .BuildServiceProvider();

            var factory = new ApplicationReviewRepositoryFactory(serviceProvider);

            // Act
            var repo = factory.GetRepository(RepositoryType.Sql);

            // Assert
            Assert.IsInstanceOf<FakeApplicationReviewService>(repo);
        }

        [Test]
        public void GetRepository_WithMongoDbType_ReturnsMongoDbApplicationReviewRepository()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddTransient<ApplicationReviewService, FakeApplicationReviewService>()
                .AddTransient<MongoDbApplicationReviewRepository, FakeMongoDbApplicationReviewRepository>()
                .BuildServiceProvider();

            var factory = new ApplicationReviewRepositoryFactory(serviceProvider);

            // Act
            var repo = factory.GetRepository(RepositoryType.MongoDb);

            // Assert
            Assert.IsInstanceOf<FakeMongoDbApplicationReviewRepository>(repo);
        }

        [Test]
        public void GetRepository_WithInvalidType_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddTransient<ApplicationReviewService, FakeApplicationReviewService>()
                .AddTransient<MongoDbApplicationReviewRepository, FakeMongoDbApplicationReviewRepository>()
                .BuildServiceProvider();

            var factory = new ApplicationReviewRepositoryFactory(serviceProvider);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => factory.GetRepository((RepositoryType)999));
        }

        // Fake implementations for testing
        private class FakeApplicationReviewService() : ApplicationReviewService(Mock.Of<IOuterApiClient>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ApplicationReviewService>>());
        private class FakeMongoDbApplicationReviewRepository() : MongoDbApplicationReviewRepository(
            Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>(),
            Mock.Of<Microsoft.Extensions.Options.IOptions<MongoDbConnectionDetails>>());
    }
}
