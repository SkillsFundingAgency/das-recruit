using System;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview;

/// <summary>
/// Factory interface for creating instances of IApplicationReviewRepository.
/// </summary>
public interface IApplicationReviewRepositoryFactory
{
    IApplicationReviewRepository GetRepository(RepositoryType type);
}

/// <summary>
/// 
/// </summary>
/// <param name="provider"></param>
public class ApplicationReviewRepositoryFactory(IServiceProvider provider) : IApplicationReviewRepositoryFactory
{
    public IApplicationReviewRepository GetRepository(RepositoryType type)
    {
        return type switch
        {
            RepositoryType.Sql => provider.GetRequiredService<ApplicationReviewService>(),
            RepositoryType.MongoDb => provider.GetRequiredService<MongoDbApplicationReviewRepository>(),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}