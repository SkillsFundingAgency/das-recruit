using System;
using FluentAssertions.Execution;

namespace SFA.DAS.Recruit.Api.UnitTests;

public static class ItIs
{
    private static bool MatchingAssertion(Action assertion)
    {
        using var assertionScope = new AssertionScope();
        assertion();
        return assertionScope.Discard().Length == 0;
    }

    public static T EquivalentTo<T>(T obj) where T : class
    {
        return It.Is<T>(seq => MatchingAssertion(() => seq.Should().BeEquivalentTo(obj, "")));
    }
}