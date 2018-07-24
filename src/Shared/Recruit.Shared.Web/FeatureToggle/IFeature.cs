namespace Esfa.Recruit.Shared.Web.FeatureToggle
{
    public interface IFeature
    {
        bool IsFeatureEnabled(string feature);
    }
}
