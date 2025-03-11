using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SFA.DAS.Recruit.Api.Extensions
{
    public static class StringExtensions
    {
        public static string RegexReplaceWithGroups(this string input, string pattern, string replacement)
        {
            var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return input;
            }
            
            var substitutions = new List<object>();
            int index = 1;
            while (match.Groups.TryGetValue(index.ToString(), out Group group))
            {
                substitutions.Add(group.Value);
                index++;
            }

            return string.Format(replacement, substitutions.ToArray());
        }
    }
}
