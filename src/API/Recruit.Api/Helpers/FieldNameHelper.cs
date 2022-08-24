namespace SFA.DAS.Recruit.Api.Helpers
{
    public static class FieldNameHelper
    {
        public static string ToCamelCasePropertyName(string input) => string.Concat(input.Substring(0, 1).ToLower(), input.Substring(1));
    }
}