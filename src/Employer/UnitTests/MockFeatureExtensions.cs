using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;

namespace Esfa.Recruit.Employer.UnitTests;

public static class MockFeatureExtensions
{
    public static void Enable(this Mock<IFeature> feature, string featureName)
    {
        ArgumentNullException.ThrowIfNull(feature);
        ArgumentNullException.ThrowIfNull(featureName);
        
        feature.Setup(x => x.IsFeatureEnabled(featureName)).Returns(true);
    }
    
    public static void Enable(this Mock<IFeature> feature, List<string> enabledFeatures)
    {
        ArgumentNullException.ThrowIfNull(feature);
        ArgumentNullException.ThrowIfNull(enabledFeatures);
        
        enabledFeatures.ForEach(feature.Enable);
    }
}