namespace Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;

public interface IFeature
{
    bool IsFeatureEnabled(string feature);
}