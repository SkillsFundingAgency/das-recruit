using System;
namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class StringExtensions
    {
        public static string ToDateQueryString(this string value)
        {
            return value.Replace("/", string.Empty);
        }
    }
}
