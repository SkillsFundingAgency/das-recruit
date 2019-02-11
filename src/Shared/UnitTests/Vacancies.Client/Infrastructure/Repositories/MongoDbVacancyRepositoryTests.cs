using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Repositories
{
    public class MongoDbVacancyRepositoryTests
    {
        [Fact]
        public void GetVacanciesByStatusAsync_ShouldRetryOnError()
        {
            var warnings = new List<string>();

            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.Log<object>(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()))
                .Callback((LogLevel l, EventId e, object s, Exception ex, Func<object, Exception, string> f) 
                    => warnings.Add(s.ToString()));

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockOptions = new Mock<IOptions<MongoDbConnectionDetails>>();

            var repo = new TestMongoDbVacancyRepository(mockLoggerFactory.Object, mockOptions.Object);

            Func<Task<IEnumerable<VacancyIdentifier>>> funcGetVacanciesByStatusAsync = () 
                => repo.GetVacanciesByStatusAsync<VacancyIdentifier>(VacancyStatus.Live);

            funcGetVacanciesByStatusAsync.Should().Throw<MongoException>();

            warnings.Count.Should().Be(3);
            warnings[0].Should().Be("Error executing Mongo Command for method GetVacanciesByProviderAccountAsync Reason: test. Retrying in 1 secs...attempt: 1") ;
            warnings[1].Should().Be("Error executing Mongo Command for method GetVacanciesByProviderAccountAsync Reason: test. Retrying in 2 secs...attempt: 2");
            warnings[2].Should().Be("Error executing Mongo Command for method GetVacanciesByProviderAccountAsync Reason: test. Retrying in 4 secs...attempt: 3");
        }

        internal class TestMongoDbVacancyRepository : MongoDbVacancyRepository
        {
            public TestMongoDbVacancyRepository(ILoggerFactory loggerFactory, IOptions<MongoDbConnectionDetails> details)
                : base(loggerFactory, details){}

            protected override IMongoCollection<T> GetCollection<T>()
            {
                var mockMongoCollection = new Mock<IFilteredMongoCollection<T>>();

                mockMongoCollection.Setup(c => c.FindAsync<T>(
                        It.IsAny<FilterDefinition<T>>(), 
                        It.IsAny<FindOptions<T>>(),
                        It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new MongoException("test"));

                return mockMongoCollection.Object;
            }
        }
    }
}
