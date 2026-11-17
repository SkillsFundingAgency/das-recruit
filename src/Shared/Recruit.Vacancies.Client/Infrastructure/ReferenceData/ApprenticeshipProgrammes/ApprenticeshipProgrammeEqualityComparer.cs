using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;

public class ApprenticeshipProgrammeEqualityComparer : IEqualityComparer<ApprenticeshipProgramme>
{
    public bool Equals(ApprenticeshipProgramme x, ApprenticeshipProgramme y)
    {
        return x.Id.Equals(y.Id)
               && x.ApprenticeshipLevel == y.ApprenticeshipLevel
               && x.ApprenticeshipType == y.ApprenticeshipType;
    }

    public int GetHashCode(ApprenticeshipProgramme obj)
    {
        return obj.Id.GetHashCode();
    }
}