using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application
{
    public static class TestData
    {
        public static IEnumerable<object[]> BlacklistedCharacters => "<>|^".ToCharArray().Select(c => new object[] { c });
    }
}
