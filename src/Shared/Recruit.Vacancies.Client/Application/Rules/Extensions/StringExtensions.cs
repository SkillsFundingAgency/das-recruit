using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.Extensions;

public static class StringExtensions
{
    public static string FormatForParsing(this string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var rgx = new Regex("[^a-zA-Z0-9]");
        var sanitized = rgx.Replace(value, " ");

        sanitized = RemoveContiguousWhitespace(sanitized);

        return $" {sanitized} ";
    }

    public static string FormatForComparison(this string value, string[] stopWords = null)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var formatted = $" {value}"
            .ToLower()
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("\t", " ");

        var builder = new StringBuilder(formatted);

        foreach (var stopWord in stopWords ?? Enumerable.Empty<string>())
            builder.Replace($" {stopWord} ", " ");

        formatted = builder.ToString();

        formatted = formatted.FormatForParsing();

        return $" {formatted} ".Trim().ToLower();
    }

    public static string ToDelimitedString<T>(this IEnumerable<T> items, string delimiter)
    {
        return items == null ? string.Empty : string.Join(delimiter, items);
    }
        
    private static string RemoveContiguousWhitespace(string value)
    {
        return Regex.Replace(value, @"\s+", " ");
    }

    public static int CountOccurrences(this string body, string term)
    {
        var count = 0;
        var offset = 0;

        if (string.IsNullOrWhiteSpace(body) || string.IsNullOrWhiteSpace(term)) return count;

        var paddedTerm = $" {term} ";
        var checkBody = $" {body} ";

        while ((offset = checkBody.IndexOf(paddedTerm, offset, StringComparison.InvariantCultureIgnoreCase)) != -1)
        {
            offset += term.Length;
            count++;
        }

        return count;
    }
}