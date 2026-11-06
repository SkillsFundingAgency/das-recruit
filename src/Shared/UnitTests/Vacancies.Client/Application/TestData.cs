using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application;

public static class TestData
{
    public static IEnumerable<object[]> BlacklistedCharacters => "<>|^".ToCharArray().Select(c => new object[] { c });
}