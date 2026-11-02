namespace Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;

public sealed class RuleSetOptions
{
    public int ReferralThreshold { get; }
    public int ApprovalThreshold { get; }

    public static readonly RuleSetOptions ZeroTolerance = new RuleSetOptions(0, 1);

    /// <summary>Options used to determine the refer/approve thresholds which are used when calculating the ruleset outcome.
    /// This allows for stringency or leniency to be applied.
    /// Basically, lower scores are better.
    /// <para>A low <paramref name="referralThreshold"></paramref> value means there's more chance of a refer outcome as 
    /// any score that are above this value will be a referral.</para>
    /// <para>A high <paramref name="referralThreshold"></paramref> value means there's more chance of an approve outcome.
    /// Any scores between these thresholds will result in an indeterminate outcome.</para>
    /// </summary>
    /// <param name="referralThreshold">If the score is ABOVE this value then the outcome will be to refer</param>
    /// <param name="approvalThreshold">If the score is BELOW this value then the outcome will be to approve</param>
    public RuleSetOptions(int referralThreshold, int approvalThreshold)
    {
        ReferralThreshold = referralThreshold;
        ApprovalThreshold = approvalThreshold;
    }
}