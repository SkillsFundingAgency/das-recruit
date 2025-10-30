using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Reports
{
    public class CsvBuilder : ICsvBuilder
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";

        public void WriteCsvToStream(
            Stream stream,
            JArray rows,
            IEnumerable<KeyValuePair<string, string>> headers,
            Func<string, ReportDataType> dataTypeResolver)
        {
            using var streamWriter = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
            using var csv = GetCsvWriter(streamWriter);

            if (rows == null || rows.Count == 0)
            {
                WriteNoResults(csv, headers);
            }
            else
            {
                // ✅ Write the meta + header once
                WriteHeader(csv, rows.First, headers);

                // ✅ Then only write data rows (no more headers)
                WriteValues(csv, rows, dataTypeResolver, writeHeader: false);
            }

            csv.Flush();
            streamWriter.Flush();
            stream.Position = 0;
        }

        private static CsvWriter GetCsvWriter(TextWriter streamWriter)
            => new CsvWriter(streamWriter, CultureInfo.GetCultureInfo("en-GB"), true);

        private void WriteNoResults(CsvWriter csv, IEnumerable<KeyValuePair<string, string>> headers)
            => WriteTotalHeader(csv, headers);

        private void WriteTotalHeader(CsvWriter csv, IEnumerable<KeyValuePair<string, string>> headers)
        {
            headers ??= Array.Empty<KeyValuePair<string, string>>();

            csv.WriteField("OFFICIAL");
            csv.NextRecord();
            csv.NextRecord();
            csv.NextRecord();

            if (headers.Any())
            {
                foreach (var kvp in headers)
                    csv.WriteField(kvp.Key);
                csv.NextRecord();

                foreach (var kvp in headers)
                    csv.WriteField(kvp.Value);
                csv.NextRecord();
            }
        }

        private void WriteHeader(
            CsvWriter csv,
            JToken row,
            IEnumerable<KeyValuePair<string, string>> headers)
        {
            // ✅ Write the meta-header only once per report
            WriteTotalHeader(csv, headers);
            csv.NextRecord();

            // ✅ Then write property names once
            foreach (var field in row.Children<JProperty>())
            {
                csv.WriteField(field.Name);
            }
            csv.NextRecord();
        }

        private void WriteValues(
            CsvWriter csv,
            JArray rows,
            Func<string, ReportDataType> dataTypeResolver,
            bool writeHeader = false)
        {
            if (rows == null || rows.Count == 0)
                return;

            // ✅ No more header calls here
            foreach (var row in rows)
            {
                foreach (var field in ((JObject)row).Properties())
                {
                    WriteField(csv, field, dataTypeResolver);
                }
                csv.NextRecord();
            }
        }

        private void WriteField(CsvWriter csv, JProperty field, Func<string, ReportDataType> dataTypeResolver)
        {
            try
            {
                var format = dataTypeResolver(field.Name);
                string value;

                if (field.Value.Type == JTokenType.Null)
                {
                    csv.WriteField(string.Empty);
                    return;
                }

                switch (format)
                {
                    case ReportDataType.DateType:
                        value = TryFormatDate(field.Value, DateFormat);
                        break;

                    case ReportDataType.DateTimeType:
                        value = TryFormatDate(field.Value, DateTimeFormat);
                        break;

                    case ReportDataType.StringType:
                        value = field.Value.Type == JTokenType.String
                            ? field.Value.ToString()
                            : field.Value.ToString(Newtonsoft.Json.Formatting.None);
                        break;

                    case ReportDataType.ArrayType:
                        if (field.Value is JArray array && array.Any())
                        {
                            var items = array
                                .Select(c => JsonConvert.DeserializeObject<ReviewField>(c.ToString())?.FieldIdentifier)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .ToDelimitedString("|");
                            value = items;
                        }
                        else
                        {
                            value = string.Empty;
                        }
                        break;

                    default:
                        value = field.Value.ToString();
                        break;
                }

                csv.WriteField(value);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error writing field '{field.Name}': {e.Message}");
                csv.WriteField(string.Empty);
            }
        }

        private static string TryFormatDate(JToken token, string format)
        {
            if (token == null || token.Type == JTokenType.Null || string.IsNullOrWhiteSpace(token.ToString()))
                return string.Empty;

            return DateTime.TryParse(token.ToString(), out var date)
                ? date.ToUkTime().ToString(format)
                : string.Empty;
        }

        private class ReviewField
        {
            public string FieldIdentifier { get; set; }
        }
    }
}