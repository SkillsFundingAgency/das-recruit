using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Extensions;

public static class ApprenticeshipTypesExtensions
{
    public static bool IsFoundation(this ApprenticeshipTypes? apprenticeshipType)
    {
        return (apprenticeshipType ?? ApprenticeshipTypes.Standard).IsFoundation();
    }
    
    public static bool IsFoundation(this ApprenticeshipTypes apprenticeshipType)
    {
        return apprenticeshipType == ApprenticeshipTypes.Foundation;
    }
}